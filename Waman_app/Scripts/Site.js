const hamburger = document.querySelector('.header__hamburger');
if (hamburger) {
    hamburger.addEventListener('click', function () {
        const isOpen = document.body.classList.toggle('menu-open');
        hamburger.setAttribute('aria-expanded', isOpen ? 'true' : 'false');
        hamburger.setAttribute('aria-label', isOpen ? 'Close navigation' : 'Open navigation');
    });
}

function ShowMessage(title, message) {
    const modalElement = document.getElementById('modal');
    const titleElement = document.getElementById('modalTitle');
    const bodyElement = document.getElementById('modalBody');
    if (!modalElement || typeof bootstrap === 'undefined') {
        window.alert(message);
        return;
    }
    titleElement.textContent = title;
    bodyElement.textContent = message;
    bootstrap.Modal.getOrCreateInstance(modalElement).show();
}

function OpenIntent(inspectorId) {
    window.location.href = `myapp://myapp/startInspection?inspectorId=${inspectorId}`;
}

function getCookie(name) {
    const value = `; ${document.cookie}`;
    const parts = value.split(`; ${name}=`);
    if (parts.length === 2) return parts.pop().split(';').shift();
}

document.addEventListener('DOMContentLoaded', function () {
    document.querySelectorAll('.ph.ui-icon').forEach(function (icon) {
        icon.setAttribute('aria-hidden', 'true');
        icon.setAttribute('focusable', 'false');
    });

    const progress = document.createElement('div');
    progress.className = 'app-navigation-progress';
    progress.setAttribute('aria-hidden', 'true');
    document.body.appendChild(progress);

    function showNavigationProgress() {
        document.body.classList.add('is-navigating');
    }

    document.addEventListener('click', function (event) {
        if (event.defaultPrevented || event.button !== 0 || event.ctrlKey || event.metaKey || event.shiftKey || event.altKey) return;
        const anchor = event.target.closest('a[href]');
        if (!anchor || anchor.target === '_blank' || anchor.hasAttribute('download')) return;

        const href = anchor.getAttribute('href');
        if (!href || href.charAt(0) === '#' || href.indexOf('javascript:') === 0) return;

        const destination = new URL(anchor.href, window.location.href);
        if (destination.origin === window.location.origin) showNavigationProgress();
    });

    document.addEventListener('submit', showNavigationProgress);
    window.addEventListener('beforeunload', showNavigationProgress);
    window.addEventListener('pageshow', function () {
        document.body.classList.remove('is-navigating');
    });

    const openPlanningsCount = parseInt(getCookie('OpenPlanningsCount'), 10) || 0;
    if (openPlanningsCount === 0) return;

    const excludedPages = ['/Home/Dashboard', '/Home/InfoBox', '/Home/InspectionArchive', '/Home/InspectionReport'];
    const shouldCheckTracking = !excludedPages.some(function (path) {
        return window.location.pathname.includes(path);
    });
    if (!shouldCheckTracking) return;

    fetch('/Home/CheckCurrentTracking')
        .then(function (response) {
            if (!response.ok) throw new Error('Tracking status request failed.');
            return response.json();
        })
        .then(function (data) {
            const startPopup = document.querySelector('.gps-popup-start');
            const restartPopup = document.querySelector('.gps-popup-blocked');
            if (!startPopup || !restartPopup) return;
            if (!data.trackingExists) {
                OpenPopup('.gps-popup-start');
            } else {
                startPopup.style.display = 'none';
                restartPopup.style.display = 'none';
            }
        })
        .catch(function (error) {
            console.error('Error checking tracking status:', error);
        });
});

function OpenPopup(selector) {
    const popup = document.querySelector(selector);
    if (!popup) return;
    popup.style.display = 'flex';
    popup.style.visibility = 'visible';
    popup.style.opacity = '0';
    popup.setAttribute('aria-hidden', 'false');

    requestAnimationFrame(function () {
        popup.style.opacity = '1';
        const signatureCanvas = popup.querySelector('canvas');
        if (signatureCanvas && typeof signatureCanvas.prepareSignatureCanvas === 'function') {
            signatureCanvas.prepareSignatureCanvas();
        }
    });
    document.body.classList.add('no-scroll');
}

function ClosePopup(selector) {
    const popup = document.querySelector(selector);
    if (!popup) return;
    popup.style.opacity = '0';
    popup.setAttribute('aria-hidden', 'true');
    window.setTimeout(function () {
        popup.style.visibility = 'hidden';
        popup.style.display = 'none';
    }, 300);
    document.body.classList.remove('no-scroll');
}

(function initialiseEntitySignature() {
    const canvas = document.getElementById('sig-canvas');
    if (!canvas) return;

    const ctx = canvas.getContext('2d');
    const sigImage = document.getElementById('sig-image');
    const boxSignature = document.getElementById('box-signature');
    const sigSubmitBtn = document.getElementById('sig-submitBtn');
    const sigClearBtn = document.getElementById('sig-clearBtn');
    let drawing = false;
    let hasInk = false;

    canvas.prepareSignatureCanvas = function () {
        const rect = canvas.getBoundingClientRect();
        if (!rect.width || !rect.height) return;
        const ratio = Math.max(window.devicePixelRatio || 1, 1);
        canvas.width = Math.round(rect.width * ratio);
        canvas.height = Math.round(rect.height * ratio);
        ctx.setTransform(ratio, 0, 0, ratio, 0, 0);
        ctx.strokeStyle = '#172033';
        ctx.lineWidth = 2.25;
        ctx.lineCap = 'round';
        ctx.lineJoin = 'round';
        hasInk = false;
    };

    canvas.addEventListener('pointerdown', function (event) {
        event.preventDefault();
        drawing = true;
        canvas.setPointerCapture(event.pointerId);
        const rect = canvas.getBoundingClientRect();
        ctx.beginPath();
        ctx.moveTo(event.clientX - rect.left, event.clientY - rect.top);
    });
    canvas.addEventListener('pointermove', function (event) {
        if (!drawing) return;
        event.preventDefault();
        const rect = canvas.getBoundingClientRect();
        ctx.lineTo(event.clientX - rect.left, event.clientY - rect.top);
        ctx.stroke();
        hasInk = true;
    });
    ['pointerup', 'pointercancel', 'pointerleave'].forEach(function (eventName) {
        canvas.addEventListener(eventName, function () { drawing = false; });
    });

    sigClearBtn.addEventListener('click', function () {
        ctx.clearRect(0, 0, canvas.width, canvas.height);
        hasInk = false;
        sigImage.removeAttribute('src');
        sigImage.style.display = 'none';
        boxSignature.style.display = 'block';
    });
    sigSubmitBtn.addEventListener('click', function () {
        if (!hasInk) {
            ShowMessage('Signature required', 'Please sign in the box before confirming.');
            return;
        }
        sigImage.src = canvas.toDataURL('image/png');
        sigImage.style.display = 'block';
        boxSignature.style.display = 'none';
        ClosePopup('.signature-popup');
    });
})();

function saveTrackingAndOpenApp(inspectorId, popupSelector, actionLabel) {
    ClosePopup(popupSelector);
    fetch('/Home/SaveStartInspectionData', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' }
    })
        .then(function (response) {
            if (!response.ok) throw new Error('Tracking request failed.');
            OpenIntent(inspectorId);
        })
        .catch(function (error) {
            console.error(`Error ${actionLabel} inspection:`, error);
            ShowMessage('Tracking error', `Failed to ${actionLabel} GPS tracking.`);
        });
}

function ClosePopupAndStartTracking(inspectorId) {
    saveTrackingAndOpenApp(inspectorId, '.gps-popup-start', 'start');
}

function ClosePopupAndRestartTracking(inspectorId) {
    saveTrackingAndOpenApp(inspectorId, '.gps-popup-blocked', 'restart');
}

document.addEventListener('DOMContentLoaded', function () {
    const submitButton = document.getElementById('submitInspectionEnd');
    if (!submitButton) return;

    submitButton.addEventListener('click', async function () {
        const form = document.getElementById('inspectionEndForm');
        const signatureField = document.getElementById('sig-image');
        const representativeField = document.querySelector('.existEntityRappresentative:checked');
        const absenceField = document.querySelector('.absent-comments');
        const representativeIsPresent = representativeField && representativeField.value === 'true';
        const absenceComment = absenceField ? absenceField.value.trim() : '';

        if (representativeIsPresent && !signatureField.getAttribute('src')) {
            ShowMessage('Signature required', 'Please provide a signature before submitting.');
            return;
        }
        if (!representativeIsPresent && absenceComment.length < 10) {
            ShowMessage('Comment required', 'Please add a comment of at least 10 characters.');
            absenceField.focus();
            return;
        }

        let signatureInput = form.querySelector('input[name="signature"]');
        if (!signatureInput) {
            signatureInput = document.createElement('input');
            signatureInput.type = 'hidden';
            signatureInput.name = 'signature';
            form.appendChild(signatureInput);
        }
        signatureInput.value = signatureField.getAttribute('src') || '';

        const originalLabel = submitButton.innerHTML;
        submitButton.disabled = true;
        submitButton.textContent = 'SAVING…';
        try {
            const response = await fetch(form.action, {
                method: 'POST',
                body: new FormData(form)
            });
            if (!response.ok) throw new Error('Save request failed.');

            const result = await response.json();
            if (!result.success) {
                ShowMessage('Unable to close inspection', result.message || 'An unexpected error occurred.');
                return;
            }
            if (result.stopIntent) {
                window.location.href = result.stopIntent;
                window.setTimeout(function () {
                    window.location.href = '/Home/Dashboard';
                }, 900);
            } else {
                window.location.href = '/Home/Dashboard';
            }
        } catch (error) {
            console.error('Error closing inspection:', error);
            ShowMessage('Connection error', 'The inspection could not be saved. Check the connection and try again.');
        } finally {
            submitButton.disabled = false;
            submitButton.innerHTML = originalLabel;
        }
    });
});
