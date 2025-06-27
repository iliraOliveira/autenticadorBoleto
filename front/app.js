document.addEventListener('DOMContentLoaded', function () {
    const form = document.getElementById('boletoForm');
    const resultDiv = document.getElementById('result');
    const barcodeLabel = document.getElementById('barcodeLabel');
    const barcodeImg = document.getElementById('barcodeImg');
    const errorMsg = document.getElementById('errorMsg');
    const validateBtn = document.getElementById('validateBtn');

    form.addEventListener('submit', async (e) => {
        e.preventDefault();
        resultDiv.style.display = 'none';
        errorMsg.style.display = 'none';
        barcodeImg.style.display = 'none';
        barcodeLabel.classList.remove('valid-barcode', 'invalid-barcode');
        validateBtn.style.display = 'none';
        validateBtn.disabled = true;

        const dataVencimento = document.getElementById('dataVencimento').value;
        const valor = document.getElementById('valor').value;

        if (!dataVencimento || !valor) {
            errorMsg.textContent = 'Preencha todos os campos.';
            errorMsg.style.display = 'block';
            return;
        }

        try {
            const response = await fetch('http://localhost:7298/api/barcode-generate', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({
                    dataVencimento,
                    valor: valor.replace(',', '.')
                })
            });
            if (!response.ok) throw new Error('Erro ao gerar código de barras.');
            const data = await response.json();
            barcodeLabel.textContent = data.barcode || '';
            if (data.imagemBase64) {
                barcodeImg.src = `data:image/png;base64,${data.imagemBase64}`;
                barcodeImg.style.display = 'block';
            } else {
                barcodeImg.style.display = 'none';
            }
            if (data.barcode) {
                validateBtn.style.display = 'inline-block';
                validateBtn.disabled = false;
            }
            resultDiv.style.display = 'block';
        } catch (err) {
            errorMsg.textContent = err.message || 'Erro inesperado.';
            errorMsg.style.display = 'block';
        }
    });

    validateBtn.addEventListener('click', async () => {
        errorMsg.style.display = 'none';
        barcodeLabel.classList.remove('valid-barcode', 'invalid-barcode');
        validateBtn.disabled = true;
        const codigoBarras = barcodeLabel.textContent.trim();
        if (!codigoBarras) return;
        try {
            const response = await fetch('http://localhost:7031/api/barcode-validate', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ codigoBarras })
            });
            if (!response.ok) throw new Error('Erro ao validar código de barras.');
            const data = await response.json();
            if (data.valido === true) {
                barcodeLabel.classList.add('valid-barcode');
                barcodeLabel.classList.remove('invalid-barcode');
            } else {
                barcodeLabel.classList.add('invalid-barcode');
                barcodeLabel.classList.remove('valid-barcode');
            }
        } catch (err) {
            errorMsg.textContent = err.message || 'Erro inesperado na validação.';
            errorMsg.style.display = 'block';
        } finally {
            validateBtn.disabled = false;
        }
    });
});
