Tras el último Pull Request, se han detectado estos errores. Te los listo a continuación:

No se ha implementado bien las Valoraciones y Comentarios de los usuarios.

Fijate en: https://github.com/joseluisgs/TiendaDawWeb-SpringBoot/blob/main/src/main/resources/templates/producto.peb.html

y en su controlador correspondiente.
https://github.com/joseluisgs/TiendaDawWeb-SpringBoot/blob/main/src/main/java/dev/joseluisgs/waladaw/controllers/RatingController.java

Cualquier usuario puede valorar cualquier producto sin haberlo comprado.
Una vez que un usuario ha valorado un producto, no puede volver a valorarlo.
Si no esta logueado, no puede valorar ni comentar.

Debes testear que todo funciona correctamente

Arregla esos errores y en un nuevo Pull Request.