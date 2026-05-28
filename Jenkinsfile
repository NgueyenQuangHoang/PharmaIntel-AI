// =============================================================================
// Jenkinsfile - PharmaIntel AI auto-deploy pipeline
//
// Tien de:
//   - Jenkins chay TREN CHINH server deploy (Ubuntu)
//   - User chay Jenkins (mac dinh `jenkins`) da duoc them vao group `docker`
//   - File .env prod nam san o /opt/pharmaintel/.env (sua bien ENV_FILE neu khac)
//   - Image GHCR public, hoac server da `docker login ghcr.io` 1 lan tu truoc
//
// Workflow:
//   1. Checkout code tu master
//   2. Pull image moi nhat tu GHCR
//   3. `docker compose up -d` (chi recreate container co image moi)
//   4. Doi API ready qua /health/ready (timeout 90s)
//   5. Neu fail -> bao loi (khong auto rollback - thao tac tay an toan hon)
//   6. Cleanup image cu khong dung
//
// Trigger:
//   - GitHub webhook push master (cau hinh trong job UI: "GitHub hook trigger")
//   - Hoac bam "Build Now" tay
// =============================================================================

pipeline {
    agent any

    options {
        timestamps()
        timeout(time: 15, unit: 'MINUTES')
        disableConcurrentBuilds()
        buildDiscarder(logRotator(numToKeepStr: '20'))
    }

    environment {
        PROJECT_NAME = 'pharmaintel'
        PROJECT_DIR  = '/opt/pharmaintel'
        ENV_FILE     = '/opt/pharmaintel/.env'
        COMPOSE_BASE = '/opt/pharmaintel/docker-compose.yml'
        COMPOSE_PROD = '/opt/pharmaintel/docker-compose.prod.yml'
        HEALTH_URL   = 'http://api:8080/health/ready'
        IMAGE_API    = 'ghcr.io/ngueyenquanghoang/pharmaintel-api'
        IMAGE_WEB    = 'ghcr.io/ngueyenquanghoang/pharmaintel-web'
    }

    stages {
        stage('Checkout') {
            steps {
                checkout scm
                sh 'git log -1 --pretty=format:"%h - %an: %s"'
            }
        }

        stage('Pre-flight check') {
            steps {
                sh '''
                    set -e
                    test -f "$ENV_FILE" || { echo "Khong tim thay $ENV_FILE"; exit 1; }
                    test -f "$COMPOSE_BASE" || { echo "Khong tim thay $COMPOSE_BASE"; exit 1; }
                    test -f "$COMPOSE_PROD" || { echo "Khong tim thay $COMPOSE_PROD"; exit 1; }
                    docker version >/dev/null
                    docker compose version >/dev/null
                '''
            }
        }

        stage('Backup current images (for rollback)') {
            steps {
                sh '''
                    set -e
                    echo "==> Tag image hien tai thanh :previous de phong rollback"

                    # Tag api hien tai thanh :previous (neu co)
                    if docker image inspect "$IMAGE_API:latest" >/dev/null 2>&1; then
                        docker tag "$IMAGE_API:latest" "$IMAGE_API:previous"
                        echo "Da tag $IMAGE_API:latest -> :previous"
                    fi

                    # Tag web hien tai thanh :previous (neu co)
                    if docker image inspect "$IMAGE_WEB:latest" >/dev/null 2>&1; then
                        docker tag "$IMAGE_WEB:latest" "$IMAGE_WEB:previous"
                        echo "Da tag $IMAGE_WEB:latest -> :previous"
                    fi
                '''
            }
        }

        stage('Pull new images') {
            steps {
                sh '''
                    set -e
                    docker compose -p "$PROJECT_NAME" \
                        -f "$COMPOSE_BASE" -f "$COMPOSE_PROD" \
                        --env-file "$ENV_FILE" \
                        pull api web
                '''
            }
        }

        stage('Deploy') {
            steps {
                sh '''
                    set -e
                    docker compose -p "$PROJECT_NAME" \
                        -f "$COMPOSE_BASE" -f "$COMPOSE_PROD" \
                        --env-file "$ENV_FILE" \
                        up -d --no-build --remove-orphans
                '''
            }
        }

        stage('Health check') {
            steps {
                script {
                    try {
                        sh '''
                            set -e
                            echo "==> Doi API ready (timeout 90s)"
                            for i in $(seq 1 18); do
                                if curl -fsS "$HEALTH_URL" >/dev/null 2>&1; then
                                    echo "==> API ready sau ${i} lan check"
                                    exit 0
                                fi
                                echo "  [${i}/18] chua ready, doi 5s..."
                                sleep 5
                            done
                            echo "==> API KHONG ready sau 90s - se rollback"
                            exit 1
                        '''
                    } catch (Exception e) {
                        // Set flag de stage rollback xu ly
                        env.NEED_ROLLBACK = 'true'
                        error("Health check FAILED - se tu rollback ve phien ban truoc")
                    }
                }
            }
        }

        stage('Cleanup') {
            steps {
                sh '''
                    docker image prune -f
                    echo "==> Deploy thanh cong, giu image :previous de phong khi can rollback tay"
                '''
            }
        }
    }

    post {
        success {
            echo "Deploy thanh cong - build #${env.BUILD_NUMBER}"
            sh '''
                docker compose -p "$PROJECT_NAME" -f "$COMPOSE_BASE" -f "$COMPOSE_PROD" --env-file "$ENV_FILE" ps
            '''
        }
        failure {
            script {
                if (env.NEED_ROLLBACK == 'true') {
                    echo "==> BAT DAU AUTO ROLLBACK ve phien ban :previous"
                    sh '''
                        set -e

                        # Kiem tra co image :previous khong
                        if ! docker image inspect "$IMAGE_API:previous" >/dev/null 2>&1; then
                            echo "Khong co image :previous de rollback (lan deploy dau tien?)"
                            exit 1
                        fi

                        # Tag :previous thanh :latest de docker compose dung
                        docker tag "$IMAGE_API:previous" "$IMAGE_API:latest"
                        docker tag "$IMAGE_WEB:previous" "$IMAGE_WEB:latest"
                        echo "Da revert :previous -> :latest"

                        # Deploy lai voi image cu
                        docker compose -p "$PROJECT_NAME" \
                            -f "$COMPOSE_BASE" -f "$COMPOSE_PROD" \
                            --env-file "$ENV_FILE" \
                            up -d --no-build --remove-orphans

                        # Cho health check lai (60s)
                        echo "==> Cho rollback ready (60s)"
                        for i in $(seq 1 12); do
                            if curl -fsS "$HEALTH_URL" >/dev/null 2>&1; then
                                echo "==> ROLLBACK THANH CONG - he thong da ve phien ban truoc"
                                exit 0
                            fi
                            sleep 5
                        done

                        echo "==> ROLLBACK CUNG FAIL - can can thiep tay gap"
                        docker compose -p "$PROJECT_NAME" -f "$COMPOSE_BASE" -f "$COMPOSE_PROD" --env-file "$ENV_FILE" ps
                        docker compose -p "$PROJECT_NAME" -f "$COMPOSE_BASE" -f "$COMPOSE_PROD" --env-file "$ENV_FILE" logs --tail=80 api
                        exit 1
                    '''
                } else {
                    echo "Deploy fail truoc khi den Health check - khong can rollback"
                }
            }
        }
    }

