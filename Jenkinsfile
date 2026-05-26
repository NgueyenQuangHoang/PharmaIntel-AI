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
        HEALTH_URL   = 'http://localhost:5292/health/ready'
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
                    echo "==> Kiem tra .env file"
                    test -f "$ENV_FILE" || { echo "Khong tim thay $ENV_FILE"; exit 1; }

                    echo "==> Kiem tra compose files"
                    test -f "$COMPOSE_BASE" || { echo "Khong tim thay $COMPOSE_BASE"; exit 1; }
                    test -f "$COMPOSE_PROD" || { echo "Khong tim thay $COMPOSE_PROD"; exit 1; }

                    echo "==> Kiem tra Docker"
                    docker version >/dev/null
                    docker compose version >/dev/null
                '''
            }
        }

        stage('Pull images') {
            steps {
                sh '''
                    set -e
                    docker compose \
                        -p "$PROJECT_NAME" \
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
                    docker compose \
                        -p "$PROJECT_NAME" \
                        -f "$COMPOSE_BASE" -f "$COMPOSE_PROD" \
                        --env-file "$ENV_FILE" \
                        up -d --no-build --remove-orphans
                '''
            }
        }

        stage('Health check') {
            steps {
                sh '''
                    set -e
                    echo "==> Doi API ready (timeout 90s)"
                    for i in $(seq 1 18); do
                        if curl -fsS "$HEALTH_URL" >/dev/null 2>&1; then
                            echo "==> API ready sau ${i} lan check (${i}x5s)"
                            exit 0
                        fi
                        echo "  [${i}/18] chua ready, doi 5s..."
                        sleep 5
                    done
                    echo "==> API KHONG ready sau 90s"
                    docker compose -p "$PROJECT_NAME" -f "$COMPOSE_BASE" -f "$COMPOSE_PROD" --env-file "$ENV_FILE" ps
                    docker compose -p "$PROJECT_NAME" -f "$COMPOSE_BASE" -f "$COMPOSE_PROD" --env-file "$ENV_FILE" logs --tail=80 api
                    exit 1
                '''
            }
        }

        stage('Cleanup') {
            steps {
                sh 'docker image prune -f'
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
            echo "Deploy that bai - kiem tra log o tren. Container hien tai van giu nguyen state."
        }
    }
}
