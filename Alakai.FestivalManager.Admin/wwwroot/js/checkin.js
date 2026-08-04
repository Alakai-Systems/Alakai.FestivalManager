window.ticketCheckIn = (function () {
    let html5QrCode = null;
    let dotNetRef = null;
    let libraryLoadingPromise = null;

    function ensureLibraryLoaded() {
        if (window.Html5Qrcode) {
            return Promise.resolve();
        }

        if (!libraryLoadingPromise) {
            libraryLoadingPromise = new Promise((resolve, reject) => {
                const script = document.createElement('script');
                script.src = 'https://unpkg.com/html5-qrcode@2.3.8/html5-qrcode.min.js';
                script.onload = () => resolve();
                script.onerror = () => reject(new Error('Could not load the QR scanning library.'));
                document.head.appendChild(script);
            });
        }

        return libraryLoadingPromise;
    }

    async function start(elementId, dotNetObjectRef) {
        await ensureLibraryLoaded();
        dotNetRef = dotNetObjectRef;

        if (html5QrCode) {
            await stop();
        }

        html5QrCode = new Html5Qrcode(elementId);

        const config = { fps: 10, qrbox: { width: 250, height: 250 } };

        await html5QrCode.start(
            { facingMode: "environment" },
            config,
            (decodedText) => {
                if (dotNetRef) {
                    dotNetRef.invokeMethodAsync('OnQrCodeScanned', decodedText);
                }
            },
            () => {
                // Errores de "no hay QR en el encuadre este frame" - esperado, se ignoran.
            }
        );
    }

    function pause() {
        if (html5QrCode) {
            try { html5QrCode.pause(true); } catch (e) { /* ignore */ }
        }
    }

    function resume() {
        if (html5QrCode) {
            try { html5QrCode.resume(); } catch (e) { /* ignore */ }
        }
    }

    async function stop() {
        if (html5QrCode) {
            try {
                await html5QrCode.stop();
                html5QrCode.clear();
            } catch (e) {
                // ignore
            }
            html5QrCode = null;
        }
        dotNetRef = null;
    }

    return { start, pause, resume, stop };
})();