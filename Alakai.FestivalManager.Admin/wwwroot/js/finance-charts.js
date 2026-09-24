window.financeCharts = (function () {
    const charts = {};

    function isDarkMode() {
        try {
            return document.documentElement.getAttribute('data-mode') === 'dark'
                || document.body.classList.contains('dark');
        } catch (e) {
            return false;
        }
    }

    function render(elementId, type, series, categories, options) {
        const el = document.getElementById(elementId);

        if (!el) {
            return;
        }

        if (charts[elementId]) {
            try { charts[elementId].destroy(); } catch (e) { /* ignore */ }
            delete charts[elementId];
        }

        const opts = options || {};

        const config = {
            chart: {
                type: type,
                height: opts.height || 260,
                toolbar: { show: false },
                fontFamily: 'inherit'
            },
            theme: { mode: isDarkMode() ? 'dark' : 'light' },
            series: series,
            xaxis: { categories: categories },
            colors: opts.colors || ['#7c3aed'],
            dataLabels: opts.dataLabelExtra ? {
                enabled: true,
                style: { fontSize: '11px', colors: [isDarkMode() ? '#e5e7eb' : '#374151'] },
                formatter: function (val, ctx) {
                    // Dos lineas (ApexCharts soporta un array de strings aqui y
                    // dibuja cada una en su propio tspan): importe en euros arriba,
                    // el texto extra (p.ej. "3 uses") debajo, en vez de todo
                    // apelotonado en una sola linea.
                    var extra = opts.dataLabelExtra[ctx.dataPointIndex];
                    var amountLabel = val.toFixed(0) + ' €';
                    return extra ? [amountLabel, extra] : amountLabel;
                }
            } : { enabled: false, style: {} },
            stroke: { curve: 'smooth', width: 2 },
            noData: { text: 'No data' }
        };

        // IMPORTANTE: nunca pongas una clave del config a "undefined" a proposito --
        // ApexCharts asume que, si la clave EXISTE, su valor es un objeto, y revienta
        // al leer una propiedad de "undefined". Omite la clave entera en vez de
        // asignarle undefined (asi es como se rompian Revenue by pass type, Aged
        // pending payments y Discount code cost, y por lo que Revenue se quedaba en
        // blanco en Weekly/Monthly).
        if (type === 'area') {
            config.fill = { type: 'gradient', gradient: { opacityFrom: 0.45, opacityTo: 0.05 } };
        }

        if (type === 'bar') {
            config.plotOptions = {
                bar: {
                    borderRadius: 4,
                    borderRadiusApplication: 'end',
                    distributed: !!opts.distributed,
                    columnWidth: '55%'
                }
            };
            if (opts.distributed) {
                config.legend = { show: false };
            }
        }

        if (opts.yFormatterSuffix || opts.yTickAmount) {
            config.yaxis = { tickAmount: opts.yTickAmount };
            if (opts.yFormatterSuffix) {
                config.yaxis.labels = {
                    formatter: function (val) {
                        return val.toLocaleString('es-ES', { maximumFractionDigits: 0 }) + ' ' + opts.yFormatterSuffix;
                    }
                };
            }
        }

        // Las etiquetas que van ENCIMA de cada barra (dataLabelExtra) necesitan hueco,
        // o la de la barra mas alta se recorta contra el borde superior del grafico --
        // se le da un 20% mas de margen al eje solo cuando se dibuja ese texto extra.
        if (opts.dataLabelExtra && series[0] && Array.isArray(series[0].data) && series[0].data.length) {
            var maxVal = Math.max.apply(null, series[0].data.map(Number).filter(function (n) { return !isNaN(n); }));
            if (maxVal > 0) {
                config.yaxis = config.yaxis || {};
                // 30% de margen (antes 20%) porque ahora la etiqueta ocupa dos
                // lineas en vez de una, y min:0 para que el eje no arranque en
                // negativo solo por el redondeo automatico de las marcas.
                config.yaxis.max = maxVal * 1.3;
                config.yaxis.min = 0;
            }
        }

        charts[elementId] = new ApexCharts(el, config);
        charts[elementId].render();
    }

    function update(elementId, series, categories) {
        if (!charts[elementId]) {
            return;
        }

        charts[elementId].updateOptions({ xaxis: { categories: categories } });
        charts[elementId].updateSeries(series);
    }

    function destroy(elementId) {
        if (charts[elementId]) {
            try { charts[elementId].destroy(); } catch (e) { /* ignore */ }
            delete charts[elementId];
        }
    }

    function renderDonut(elementId, series, labels, options) {
        const el = document.getElementById(elementId);

        if (!el) {
            return;
        }

        if (charts[elementId]) {
            try { charts[elementId].destroy(); } catch (e) { /* ignore */ }
            delete charts[elementId];
        }

        const opts = options || {};

        const config = {
            chart: {
                type: 'donut',
                height: opts.height || 260,
                fontFamily: 'inherit'
            },
            theme: { mode: isDarkMode() ? 'dark' : 'light' },
            series: series,
            labels: labels,
            colors: opts.colors || ['#16a34a', '#f59e0b', '#dc2626'],
            dataLabels: { enabled: true },
            legend: { position: 'bottom' },
            noData: { text: 'No data' }
        };

        charts[elementId] = new ApexCharts(el, config);
        charts[elementId].render();
    }

    return { render, update, destroy, renderDonut };
})();