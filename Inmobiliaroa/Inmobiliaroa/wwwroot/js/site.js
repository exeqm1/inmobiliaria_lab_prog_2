// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

(function () {
    "use strict";

    const RETRASO_BUSQUEDA = 250;
    const MINIMO_CARACTERES = 2;
    const MAXIMO_RESULTADOS = 10;

    function inicializarCombo(contenedor) {
        const texto = contenedor.querySelector(".combo-busqueda-input");
        const oculto = contenedor.querySelector('input[type="hidden"]');
        const lista = contenedor.querySelector(".combo-busqueda-lista");
        const url = contenedor.dataset.url;

        if (!texto || !oculto || !lista || !url) {
            return;
        }

        let temporizador = null;
        let resultados = [];
        let activo = -1;
        let consultaActual = 0;

        function cerrarLista() {
            lista.innerHTML = "";
            lista.classList.remove("abierta");
            texto.setAttribute("aria-expanded", "false");
            resultados = [];
            activo = -1;
        }

        function pintarResultados() {
            lista.innerHTML = "";

            if (resultados.length === 0) {
                cerrarLista();
                return;
            }

            resultados.slice(0, MAXIMO_RESULTADOS).forEach(function (item, indice) {
                const opcion = document.createElement("button");
                opcion.type = "button";
                opcion.className = "combo-busqueda-item";
                opcion.textContent = item.text;
                opcion.dataset.indice = indice;

                opcion.addEventListener("mousedown", function (evento) {
                    evento.preventDefault();
                    elegir(item);
                });

                lista.appendChild(opcion);
            });

            lista.classList.add("abierta");
            texto.setAttribute("aria-expanded", "true");
        }

        function marcarActivo() {
            Array.prototype.forEach.call(lista.children, function (elemento, indice) {
                elemento.classList.toggle("activo", indice === activo);
            });
        }

        function elegir(item) {
            oculto.value = item.id;
            texto.value = item.text;
            cerrarLista();
        }

        function buscar(termino) {
            const consulta = ++consultaActual;
            const separador = url.indexOf("?") >= 0 ? "&" : "?";

            fetch(url + separador + "q=" + encodeURIComponent(termino), {
                headers: { "Accept": "application/json" }
            })
                .then(function (respuesta) {
                    return respuesta.ok ? respuesta.json() : [];
                })
                .then(function (datos) {
                    if (consulta !== consultaActual) {
                        return;
                    }

                    resultados = Array.isArray(datos) ? datos : [];
                    activo = -1;
                    pintarResultados();
                })
                .catch(function () {
                    cerrarLista();
                });
        }

        texto.addEventListener("input", function () {
            oculto.value = "";
            clearTimeout(temporizador);

            const termino = texto.value.trim();
            if (termino.length < MINIMO_CARACTERES) {
                cerrarLista();
                return;
            }

            temporizador = setTimeout(function () {
                buscar(termino);
            }, RETRASO_BUSQUEDA);
        });

        texto.addEventListener("keydown", function (evento) {
            if (evento.key === "Escape") {
                cerrarLista();
                return;
            }

            if (resultados.length === 0) {
                return;
            }

            if (evento.key === "ArrowDown") {
                evento.preventDefault();
                activo = (activo + 1) % resultados.length;
                marcarActivo();
            } else if (evento.key === "ArrowUp") {
                evento.preventDefault();
                activo = (activo - 1 + resultados.length) % resultados.length;
                marcarActivo();
            } else if (evento.key === "Enter" && activo >= 0) {
                evento.preventDefault();
                elegir(resultados[activo]);
            }
        });

        texto.addEventListener("focus", function () {
            if (resultados.length > 0) {
                pintarResultados();
            }
        });

        texto.addEventListener("blur", function () {
            setTimeout(cerrarLista, 120);
        });
    }

    document.addEventListener("DOMContentLoaded", function () {
        document.querySelectorAll(".combo-busqueda").forEach(inicializarCombo);
    });
})();
