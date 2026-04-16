// Digital Meal Planner — site.js

// Hamburger menu toggle
(function () {
    const btn = document.getElementById('nav-hamburger');
    const menu = document.getElementById('nav-menu');
    if (!btn || !menu) return;
    btn.addEventListener('click', () => {
        const open = btn.classList.toggle('open');
        menu.classList.toggle('open');
        btn.setAttribute('aria-expanded', open);
    });
})();

// PWA service worker registration
if ('serviceWorker' in navigator) {
    window.addEventListener('load', () => {
        navigator.serviceWorker.register('/service-worker.js').catch(() => {});
    });
}

// Image drop zone — used on recipe import page
function initDropZone(dropEl, inputEl, previewEl) {
    if (!dropEl || !inputEl) return;

    dropEl.addEventListener('dragover', e => {
        e.preventDefault();
        dropEl.classList.add('drag-over');
    });
    dropEl.addEventListener('dragleave', () => dropEl.classList.remove('drag-over'));
    dropEl.addEventListener('drop', e => {
        e.preventDefault();
        dropEl.classList.remove('drag-over');
        const file = e.dataTransfer.files[0];
        if (file) setFile(file);
    });
    dropEl.addEventListener('click', () => inputEl.click());
    inputEl.addEventListener('change', () => {
        if (inputEl.files[0]) setFile(inputEl.files[0]);
    });

    function setFile(file) {
        const dt = new DataTransfer();
        dt.items.add(file);
        inputEl.files = dt.files;
        if (previewEl) {
            const reader = new FileReader();
            reader.onload = e => {
                previewEl.src = e.target.result;
                previewEl.style.display = 'block';
            };
            reader.readAsDataURL(file);
        }
        dropEl.querySelector('.drop-label').textContent = file.name;
    }
}

document.addEventListener('DOMContentLoaded', () => {
    initDropZone(
        document.getElementById('drop-zone'),
        document.getElementById('image-input'),
        document.getElementById('image-preview')
    );
});
