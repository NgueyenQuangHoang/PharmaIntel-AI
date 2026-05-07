# =============================================================================
# Dockerfile - PharmaIntel AI frontend (React + Vite + TS)
# Multi-stage build:
#   1) build : cai dependency, compile TS + Vite build -> /app/dist (static)
#   2) final : nginx alpine, serve static + SPA fallback (history routing)
#
# Vite inline env var luc BUILD (khong phai runtime). Truyen API URL qua ARG:
#   docker build -t pharmaintel-web \
#     --build-arg VITE_API_URL=https://api.pharmaintel.example.com/api \
#     ./client
#   docker run --rm -p 8080:8080 pharmaintel-web
# =============================================================================

# ---- Build stage ----
FROM node:22-alpine AS build
WORKDIR /app

# Copy manifest truoc de cache layer `npm ci` khi chi sua source.
COPY package.json package-lock.json ./
RUN npm ci

# API URL duoc inline vao bundle luc build. Mac dinh tro ve API trong cung
# stack docker-compose; ghi de qua --build-arg khi deploy ra prod.
ARG VITE_API_URL=http://localhost:5292/api
ENV VITE_API_URL=$VITE_API_URL

COPY . .
RUN npm run build

# ---- Final runtime stage ----
FROM nginx:1.27-alpine AS final

# Cau hinh nginx tuy bien (SPA fallback, gzip, cache static).
COPY nginx.conf /etc/nginx/conf.d/default.conf

# Copy static da build sang webroot cua nginx.
COPY --from=build /app/dist /usr/share/nginx/html

# Image nginx:alpine listen 8080 (cau hinh trong nginx.conf) - non-privileged.
EXPOSE 8080

HEALTHCHECK --interval=30s --timeout=5s --start-period=10s --retries=3 \
    CMD wget -q --spider http://localhost:8080/ || exit 1

# nginx:alpine da co ENTRYPOINT/CMD mac dinh - giu nguyen.
