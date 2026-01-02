# 08. Operaciones y Producción: Docker, Ficheros y Despliegue

Tu app ya funciona. Ahora vamos a empaquetarla para que sea indestructible.

## 1. El Dockerfile Multi-Stage
Para que tu imagen de servidor no pese 1GB, dividimos el trabajo:
1. **Etapa SDK**: Una imagen pesada para compilar el código.
2. **Etapa Runtime**: Una imagen ligera (solo ejecución). Copiamos solo el resultado de la compilación.
**Resultado**: Servidores más rápidos y seguros.

## 2. Persistencia en Docker (Volumes)
Un contenedor es como un CD-ROM: no se puede escribir permanentemente en él. Si guardas una foto de producto, al reiniciar el servidor, se pierde.
**La Solución**: Volúmenes. Mapeamos la carpeta física `/mis-datos/uploads` con la carpeta virtual del servidor. Así las imágenes sobreviven a los reinicios.

## 3. Procesamiento de Imágenes (ImageSharp)
Nunca confíes en el usuario. Si sube una foto de 10MB, tu servidor morirá.
- Redimensionamos la imagen al subirla (máximo 800px).
- La comprimimos para que pese KB en lugar de MB.
Esto ahorra espacio en disco y mejora el SEO de la web.

## 4. Generación de Facturas (QuestPDF)
Generar un PDF es dibujar. Usamos la librería QuestPDF, que genera el documento en memoria de forma ultra-rápida y lo envía como un chorro de bytes al navegador.
