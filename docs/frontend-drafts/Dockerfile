# Använd en minimal och stabil Nginx-avbild baserad på Alpine Linux
FROM nginx:alpine

# Kopiera alla frontend-filer (HTML, JS, CSS, etc.) till Nginx standardmapp för statiska filer
COPY . /usr/share/nginx/html

# Exponera port 80 internt i containern
EXPOSE 80

# Starta Nginx och kör den i förgrunden för att hålla containern vid liv
CMD ["nginx", "-g", "daemon off;"]
