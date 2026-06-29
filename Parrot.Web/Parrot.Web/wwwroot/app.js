window.oriScrollToBottom = function () {
    const el = document.getElementById('ori-scroll');
    if (el) el.scrollTop = el.scrollHeight;
};

window.modalBodyLock = {
    lock: function () {
        document.documentElement.style.overflow = 'hidden';
    },
    unlock: function () {
        document.documentElement.style.overflow = '';
    }
};
