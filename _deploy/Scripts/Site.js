let hamburger = document.querySelector('.header__hamburger');
hamburger.addEventListener("click", function () {
    document.body.classList.toggle('menu-open');
});


function OpenIntent(inspectorId) {
    //const urlApp = `intent://myapp/startInspection?inspectorId=${inspectorId}#Intent;scheme=myapp;package=com.easywaytechnology.waman;action=com.easywaytechnology.waman.ACTION_START_INSPECTION;end`;
    const urlApp = `myapp://myapp/startInspection?inspectorId=${inspectorId}`;
    window.location.href = urlApp;
}


function getCookie(name) {
    const value = `; ${document.cookie}`;
    const parts = value.split(`; ${name}=`);
    if (parts.length === 2) return parts.pop().split(';').shift();
}


document.addEventListener("DOMContentLoaded", function () {
    // Primo controllo: verifica il valore di openPlanningsCount
    const openPlanningsCount = parseInt(getCookie('OpenPlanningsCount')) || 0;

    if (openPlanningsCount === 0)
        return;

    if ((!window.location.pathname.includes("/Home/Dashboard")) && (!window.location.pathname.includes("/Home/InfoBox")) && (!window.location.pathname.includes("/Home/InspectionArchive"))) {
        fetch('/Home/CheckCurrentTracking')
            .then(response => response.json())
            .then(data => {
                const startPopup = document.querySelector('.gps-popup-start');
                const restartPopup = document.querySelector('.gps-popup-blocked');

                if (!startPopup || !restartPopup)
                    return;

                // Se non esiste il tracking mostra popup di START
                if (!data.trackingExists)
                    OpenPopup('.gps-popup-start');
                else
                {
                    // trackingExists == true => controlla blocked
                    // se blocked == true o null => popup RESTART
                    if (data.blocked === true || data.blocked === null)
                        OpenPopup('.gps-popup-blocked');
                    else
                    {
                        // altrimenti nessun popup
                        startPopup.style.display = "none";
                        restartPopup.style.display = "none";
                    }
                }
            })
            .catch(err => console.error("Error checking tracking status:", err));
    }

    // 2) Se la pagina contiene "/Home/InspectionEnd", apri la popup per la firma
    if (window.location.pathname.includes("/Home/InspectionEnd"))
        OpenPopup('.signature-popup');
});


// Funzione per aprire un popup specifico
function OpenPopup(selector) {
    const popup = document.querySelector(selector);
    if (!popup) return;

    // All'inizio impostiamo lo stile a display: block e opacity 0
    popup.style.display = 'flex';
    popup.style.visibility = 'visible';
    popup.style.opacity = '0';
    popup.style.transition = 'opacity 0.3s ease';

    popup.offsetHeight;

    // Forziamo l'animazione con requestAnimationFrame
    requestAnimationFrame(() => {
        popup.style.opacity = '1'; // Fai il fade-in
    });

    // Previene lo scroll del body
    document.body.classList.add('no-scroll');
}


// Funzione per chiudere un popup specifico
function ClosePopup(selector) {
    const popup = document.querySelector(selector);
    if (!popup) return;

    popup.style.opacity = '1';
    popup.style.transition = 'opacity 0.3s ease';

    // Avviamo l'animazione di fade-out
    requestAnimationFrame(() => {
        popup.style.opacity = '0';
        setTimeout(() => {
            // Dopo la transizione nascondiamo l'elemento
            popup.style.visibility = 'hidden';
            popup.style.display = 'none';
        }, 300);
    });

    // Ripristina lo scroll sul body
    document.body.classList.remove('no-scroll');
}

(function () {

    const canvas = document.getElementById("sig-canvas");
    let ctx = null;

    if (canvas != null) {
        ctx = canvas.getContext("2d");
        ctx.strokeStyle = "#222222";
        ctx.lineWidth = 1;

        const sigImage = document.getElementById("sig-image");
        const boxSignature = document.getElementById("box-signature");
        const sigSubmitBtn = document.getElementById("sig-submitBtn");
        const sigClearBtn = document.getElementById("sig-clearBtn");

        let drawing = false;
        let lastPos = { x: 0, y: 0 };

        // Funzione per ottenere le coordinate del mouse o del tocco
        function getPosition(event) {
            const rect = canvas.getBoundingClientRect();
            if (event.touches) {
                return {
                    x: event.touches[0].clientX - rect.left,
                    y: event.touches[0].clientY - rect.top
                };
            }
            return {
                x: event.clientX - rect.left,
                y: event.clientY - rect.top
            };
        }

        // Funzione per iniziare a disegnare
        function startDrawing(event) {
            drawing = true;
            lastPos = getPosition(event);
        }

        // Funzione per fermare il disegno
        function stopDrawing() {
            drawing = false;
        }

        // Funzione per disegnare
        function draw(event) {
            if (!drawing) return;
            const pos = getPosition(event);
            ctx.beginPath();
            ctx.moveTo(lastPos.x, lastPos.y);
            ctx.lineTo(pos.x, pos.y);
            ctx.stroke();
            ctx.closePath();
            lastPos = pos;
        }

        // Eventi per mouse
        canvas.addEventListener("mousedown", startDrawing);
        canvas.addEventListener("mousemove", draw);
        canvas.addEventListener("mouseup", stopDrawing);
        canvas.addEventListener("mouseleave", stopDrawing);

        // Eventi per touch
        canvas.addEventListener("touchstart", (e) => {
            e.preventDefault();
            startDrawing(e);
        });
        canvas.addEventListener("touchmove", (e) => {
            e.preventDefault();
            draw(e);
        });
        canvas.addEventListener("touchend", (e) => {
            e.preventDefault();
            stopDrawing();
        });

        // Pulsante per cancellare la firma
        sigClearBtn.addEventListener("click", function () {
            ctx.clearRect(0, 0, canvas.width, canvas.height);
            sigImage.style.display = "none";
            canvas.style.display = "block";
            boxSignature.style.display = "block";
        });

        // Pulsante per confermare la firma
        sigSubmitBtn.addEventListener("click", function () {
            const dataUrl = canvas.toDataURL("image/png");
            sigImage.src = dataUrl;
            sigImage.style.display = "block";
            canvas.style.display = "none";
            boxSignature.style.display = "none";
            ClosePopup(".signature-popup");
        });
    }
})();



function ClosePopupAndStartTracking(inspectorId) {
    ClosePopup('.gps-popup-start');

    // Avvia il tracking salvando i dati
    fetch('/Home/SaveStartInspectionData', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        }
    }).then(response => {
        if (response.ok) {
            // Dopo aver salvato il tracking, avvia l'intent
            OpenIntent(inspectorId);
            location.reload();
        } else {
            alert('Failed to start GPS tracking.');
        }
    }).catch(err => {
        console.error('Error starting inspection:', err);
    });
}


function ClosePopupAndRestartTracking(inspectorId) {
    ClosePopup('.gps-popup-blocked');

    fetch('/Home/SaveStartInspectionData', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        }
    }).then(response => {
        if (response.ok) {
            // Avvia l'intent di Start (o di restart?)
            OpenIntent(inspectorId);
            location.reload();
        } else {
            alert('Failed to restart GPS tracking.');
        }
    }).catch(err => {
        console.error('Error restarting inspection:', err);
    });
}


//Close inspection click event
document.addEventListener("DOMContentLoaded", function () {
    const submitInspectionEndButton = document.getElementById("submitInspectionEnd");

    if (submitInspectionEndButton) {
        submitInspectionEndButton.addEventListener("click", async function () {
            const form = document.getElementById("inspectionEndForm");
            const signatureField = document.getElementById("sig-image");

            // Verifica la firma
            if (signatureField.currentSrc == "") {
                alert("Please provide a signature before submitting.");
                return;
            }

            // Aggiungi la firma al form
            const signatureDataUrl = signatureField.src;
            const signatureInput = document.createElement("input");
            signatureInput.type = "hidden";
            signatureInput.name = "signature";
            signatureInput.value = signatureDataUrl;
            form.appendChild(signatureInput);

            // Invia il form via fetch per ricevere risposta JSON dal backend
            const formData = new FormData(form);
            const response = await fetch(form.action, {
                method: "POST",
                body: formData,
            });

            if (response.ok)
            {
                const result = await response.json();

                if (result.success)
                {
                    if (result.stopIntent)
                    {
                        window.location.href = result.stopIntent;

                        // Dopo un breve ritardo, continua il flusso
                        setTimeout(() => {
                            window.location.href = "/Home/Dashboard";
                        }, 500);
                    }
                    else
                        window.location.href = "/Home/Dashboard";
                }
                else 
                    alert(result.message || "An unexpected error occurred.");
            }
            else 
                alert("Failed to save inspection. Please try again.");
        });
    }
});
