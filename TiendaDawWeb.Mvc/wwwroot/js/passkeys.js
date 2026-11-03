// ============================================================
// Passkeys de WalaDaw (FIDO2/WebAuthn).
// WebAuthn puro, sin librerías: el servidor devuelve las opciones en JSON,
// el dispositivo hace el gesto (huella, cara o PIN) y aquí solo se manda la
// credencial de vuelta como campo de formulario con el token anti-forgery,
// igual que cualquier otro POST de la aplicación.
//
// El script mira el div [data-passkeys]: si tiene data-creacion, se crea una
// passkey (zona de perfil); si tiene data-opciones, se entra con ella (acceso).
// ============================================================
(function () {
    'use strict';

    var contenedor = document.querySelector('[data-passkeys]');
    if (!contenedor) {
        return;
    }

    var aviso = contenedor.querySelector('[data-testid="aviso-passkey"]');
    var tokenCampo = contenedor.querySelector('input[name="__RequestVerificationToken"]');

    function decir(mensaje, esError) {
        if (!aviso) {
            return;
        }

        aviso.textContent = mensaje;
        aviso.className = esError
            ? 'alert alert-danger py-2 mb-0'
            : 'alert alert-success py-2 mb-0';
    }

    // POST urlencoded con el token anti-forgery: el servidor exige los dos juntos.
    function enviar(url, campos) {
        if (!tokenCampo) {
            return Promise.reject(new Error('Falta el token anti-forgery en la página'));
        }

        var datos = new URLSearchParams();
        datos.append('__RequestVerificationToken', tokenCampo.value);

        Object.keys(campos || {}).forEach(function (clave) {
            datos.append(clave, campos[clave]);
        });

        return fetch(url, {
            method: 'POST',
            headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
            body: datos.toString()
        }).then(function (respuesta) {
            return respuesta.json()
                .catch(function () { return null; })
                .then(function (cuerpo) {
                    return { ok: respuesta.ok, status: respuesta.status, cuerpo: cuerpo };
                });
        });
    }

    // Convierte los bytes de WebAuthn (ArrayBuffer o Uint8Array) a base64url,
    // que es la forma en que el protocolo transporta binario dentro de un JSON.
    function aBase64Url(valor) {
        if (!valor) {
            return undefined;
        }

        if (Array.isArray(valor)) {
            valor = Uint8Array.from(valor);
        }

        if (valor instanceof ArrayBuffer) {
            valor = new Uint8Array(valor);
        }

        if (valor instanceof Uint8Array) {
            var texto = '';
            for (var i = 0; i < valor.byteLength; i++) {
                texto += String.fromCharCode(valor[i]);
            }

            valor = window.btoa(texto);
        }

        if (typeof valor !== 'string') {
            throw new Error('No se pudo convertir la credencial a base64url');
        }

        return valor.replace(/\+/g, '-').replace(/\//g, '_').replace(/=+$/, '');
    }

    // JSON.stringify(credencial) vale cuando el navegador trae toJSON (los actuales).
    // Si no lo trae, el stringify devolvería un objeto vacío, así que se monta el JSON
    // a mano campo a campo, que es lo mismo que hace la plantilla oficial de .NET.
    function serializar(credencial) {
        if (typeof credencial.toJSON === 'function') {
            return JSON.stringify(credencial);
        }

        return JSON.stringify({
            authenticatorAttachment: credencial.authenticatorAttachment,
            clientExtensionResults: credencial.getClientExtensionResults(),
            id: credencial.id,
            rawId: aBase64Url(credencial.rawId),
            response: {
                attestationObject: aBase64Url(credencial.response.attestationObject),
                authenticatorData: aBase64Url(
                    credencial.response.authenticatorData ||
                    (credencial.response.getAuthenticatorData && credencial.response.getAuthenticatorData())),
                clientDataJSON: aBase64Url(credencial.response.clientDataJSON),
                publicKey: aBase64Url(credencial.response.getPublicKey && credencial.response.getPublicKey()),
                publicKeyAlgorithm: credencial.response.getPublicKeyAlgorithm &&
                    credencial.response.getPublicKeyAlgorithm(),
                transports: credencial.response.getTransports && credencial.response.getTransports(),
                signature: aBase64Url(credencial.response.signature),
                userHandle: aBase64Url(credencial.response.userHandle)
            },
            type: credencial.type
        });
    }

    function falloConocido(error) {
        if (error && error.name === 'NotAllowedError') {
            return 'Cancelaste el gesto o el dispositivo no respondió';
        }

        if (error && error.name === 'NotSupportedError') {
            return 'Este navegador o este equipo no admiten passkeys';
        }

        return error && error.message ? error.message : 'Error desconocido del navegador';
    }

    // Entrar con passkey: opciones con el correo (si lo hay) y después la afirmación.
    function entrar() {
        if (!window.PublicKeyCredential) {
            decir('Este navegador no soporta passkeys', true);
            return;
        }

        var campoCorreo = document.getElementById('correo-passkey');
        decir('Pidiendo las opciones al servidor...', false);

        enviar(contenedor.dataset.opciones, {
            email: campoCorreo ? campoCorreo.value : ''
        }).then(function (respuesta) {
            if (!respuesta.ok || !respuesta.cuerpo) {
                throw new Error((respuesta.cuerpo && respuesta.cuerpo.mensaje) ||
                    'No se pudieron pedir las opciones (' + respuesta.status + ')');
            }

            var opciones = PublicKeyCredential.parseRequestOptionsFromJSON(respuesta.cuerpo);
            return navigator.credentials.get({ publicKey: opciones });
        }).then(function (credencial) {
            decir('Verificando la firma en el servidor...', false);
            return enviar(contenedor.dataset.afirmacion, {
                credential: serializar(credencial),
                returnUrl: contenedor.dataset.destino || ''
            });
        }).then(function (respuesta) {
            if (respuesta.ok && respuesta.cuerpo && respuesta.cuerpo.ok) {
                window.location.href = respuesta.cuerpo.destino || '/';
                return;
            }

            decir((respuesta.cuerpo && respuesta.cuerpo.mensaje) || 'La passkey no ha valido', true);
        }).catch(function (error) {
            decir(falloConocido(error), true);
        });
    }

    // Crear passkey: opciones de creación, gesto del dispositivo y atestación guardada.
    function crear() {
        if (!window.PublicKeyCredential) {
            decir('Este navegador no soporta passkeys', true);
            return;
        }

        decir('Pidiendo las opciones de creación...', false);

        enviar(contenedor.dataset.creacion, {}).then(function (respuesta) {
            if (!respuesta.ok || !respuesta.cuerpo) {
                throw new Error((respuesta.cuerpo && respuesta.cuerpo.mensaje) ||
                    'No se pudieron pedir las opciones (' + respuesta.status + ')');
            }

            var opciones = PublicKeyCredential.parseCreationOptionsFromJSON(respuesta.cuerpo);
            return navigator.credentials.create({ publicKey: opciones });
        }).then(function (credencial) {
            decir('Registrando la passkey en el servidor...', false);
            return enviar(contenedor.dataset.atestacion, { credential: serializar(credencial) });
        }).then(function (respuesta) {
            if (respuesta.ok && respuesta.cuerpo && respuesta.cuerpo.ok) {
                window.location.reload();
                return;
            }

            decir((respuesta.cuerpo && respuesta.cuerpo.mensaje) || 'No se pudo registrar la passkey', true);
        }).catch(function (error) {
            decir(falloConocido(error), true);
        });
    }

    contenedor.querySelectorAll('[data-accion]').forEach(function (boton) {
        boton.addEventListener('click', function () {
            if (boton.dataset.accion === 'crear') {
                crear();
            } else {
                entrar();
            }
        });
    });
})();
