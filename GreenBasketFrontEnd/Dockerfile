FROM nginx:alpine

# Copy all static files into nginx
COPY . /usr/share/nginx/html

# Expose port 80
EXPOSE 80

# Default Nginx configuration is sufficient
CMD ["nginx", "-g", "daemon off;"]
