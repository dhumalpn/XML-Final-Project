(function () {
    const upcInput = document.getElementById('upcInput');
    const lookupBtn = document.getElementById('lookupBtn');
    const statusEl = document.getElementById('lookupStatus');

    if (!lookupBtn) {
        console.error('lookupBtn not found on page');
        return;
    }

    lookupBtn.addEventListener('click', async () => {
        const upc = upcInput.value?.trim();
        if (!upc) { alert('Enter a barcode/UPC'); upcInput.focus(); return; }

        statusEl.textContent = 'Looking up...';
        lookupBtn.disabled = true;

        try {
            const tokenEl = document.querySelector('input[name="__RequestVerificationToken"]');
            const token = tokenEl ? tokenEl.value : null;

            const resp = await fetch(window.location.pathname + '?handler=LookupUpc', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    ...(token ? { 'RequestVerificationToken': token } : {})
                },
                body: JSON.stringify({ upc })
            });

            if (!resp.ok) {
                statusEl.textContent = 'Lookup failed: ' + resp.statusText;
                return;
            }

            const data = await resp.json();
            if (data.found) {
                document.getElementById('autoTitle').value = data.title || '';
                document.getElementById('autoBrand').value = data.brand || '';
                document.getElementById('autoCategory').value = data.category || '';
                document.getElementById('autoNutri').value = data.nutriScore || '';
                statusEl.textContent = '✅ Product found and populated.';
            } else {
                statusEl.textContent = data.message || '❌ Product not found.';
            }
        } catch (err) {
            console.error(err);
            statusEl.textContent = '⚠️ Lookup error. See console.';
        } finally {
            lookupBtn.disabled = false;
        }
    });
})();