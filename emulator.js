window.drawDisplay = function (display, width, height, scale) {
    const canvas = document.getElementById("screen");
    if (!canvas) return;

    const ctx = canvas.getContext("2d");
    ctx.imageSmoothingEnabled = false;

    const border = 1;

    const totalWidth = (width + border * 2) * scale;
    const totalHeight = (height + border * 2) * scale;

    if (canvas.width !== totalWidth || canvas.height !== totalHeight) {
        canvas.width = totalWidth;
        canvas.height = totalHeight;
    }

    ctx.fillStyle = "black";
    ctx.fillRect(0, 0, canvas.width, canvas.height);

    ctx.fillStyle = "white";
    ctx.fillRect(0, 0, canvas.width, scale);
    ctx.fillRect(0, canvas.height - scale, canvas.width, scale);
    ctx.fillRect(0, 0, scale, canvas.height);
    ctx.fillRect(canvas.width - scale, 0, scale, canvas.height);

    for (let y = 0; y < height; y++) {
        for (let x = 0; x < width; x++) {
            if (display[y * width + x]) {
                const drawX = (x + border) * scale;
                const drawY = (y + border) * scale;
                ctx.fillRect(drawX, drawY, scale, scale);
            }
        }
    }
};

window.ensureAudio = () => {
    if (!window.audioCtx) {
        window.audioCtx = new (window.AudioContext || window.webkitAudioContext)();
    }
    if (window.audioCtx.state === "suspended") {
        window.audioCtx.resume();
    }
};

// Play or stop a simple square-wave beep used by Chip-8
window.playChip8Sound = (isPlaying) => {
    window.ensureAudio();

    const ctx = window.audioCtx;
    if (!ctx) return;

    if (isPlaying) {
        if (!window.chip8Osc) {
            const osc = ctx.createOscillator();
            const gain = ctx.createGain();
            osc.type = 'square';
            osc.frequency.value = 800; // Chip-8 beep-ish tone
            gain.gain.value = 0.08; // safe default volume
            osc.connect(gain);
            gain.connect(ctx.destination);
            osc.start();
            window.chip8Osc = osc;
            window.chip8Gain = gain;
        }
    }
    else {
        if (window.chip8Osc) {
            try {
                window.chip8Osc.stop();
            } catch (e) { }
            try { window.chip8Gain.disconnect(); } catch (e) { }
            try { window.chip8Osc.disconnect(); } catch (e) { }
            window.chip8Osc = null;
            window.chip8Gain = null;
        }
    }
};