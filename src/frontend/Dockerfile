# Build stage
FROM node:18-alpine AS build

WORKDIR /app

# Copy package files
COPY package*.json ./

# Install dependencies with legacy peer deps to resolve conflicts
RUN npm install --legacy-peer-deps

# Copy source code
COPY . .

# Build Angular app (gera dist/MBA.Modulo4)
RUN npm run build

# Production stage
FROM nginx:alpine

# OCI labels
ARG BUILD_DATE="unknown"
ARG VCS_REF="unknown"
ARG VERSION="dev"
LABEL org.opencontainers.image.title="frontend" \
      org.opencontainers.image.description="Frontend (Angular + Nginx)" \
      org.opencontainers.image.created=$BUILD_DATE \
      org.opencontainers.image.revision=$VCS_REF \
      org.opencontainers.image.version=$VERSION

# Copy built app from build stage (ajustado para nome real do output)
COPY --from=build /app/dist/MBA.Modulo4 /usr/share/nginx/html

# Copy nginx configuration
COPY nginx.conf /etc/nginx/nginx.conf

# Ajustar permissões para execução como usuário não-root
RUN mkdir -p /var/run \
    && chown -R nginx:nginx /var/cache/nginx /var/log/nginx /var/run /usr/share/nginx/html

# Expose port
EXPOSE 8080

# Start nginx
USER nginx
CMD ["nginx", "-g", "daemon off;"] 
