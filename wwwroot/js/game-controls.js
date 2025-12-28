// wwwroot/js/game.js

window.setupKeyboardControls = (dotnetHelper) => {
    // Track which keys are currently pressed to prevent repeating
    const pressedKeys = new Set();

    // Remove existing listeners if any
    if (window.keydownHandler) {
        document.removeEventListener('keydown', window.keydownHandler);
    }
    if (window.keyupHandler) {
        document.removeEventListener('keyup', window.keyupHandler);
    }

    window.keydownHandler = async (event) => {
        const key = event.key.toLowerCase();

        // Ignore if key is already pressed (prevents repeat)
        if (pressedKeys.has(key)) {
            return;
        }

        let direction = null;

        switch (key) {
            case 'w':
            case 'arrowup':
                direction = 'up';
                event.preventDefault();
                break;
            case 's':
            case 'arrowdown':
                direction = 'down';
                event.preventDefault();
                break;
            case 'a':
            case 'arrowleft':
                direction = 'left';
                event.preventDefault();
                break;
            case 'd':
            case 'arrowright':
                direction = 'right';
                event.preventDefault();
                break;
        }

        if (direction) {
            pressedKeys.add(key);
            try {
                await dotnetHelper.invokeMethodAsync('HandleKeyPress', direction);
            } catch (error) {
                console.error('Error sending key press:', error);
            }
        }
    };

    window.keyupHandler = (event) => {
        const key = event.key.toLowerCase();
        pressedKeys.delete(key);
    };

    document.addEventListener('keydown', window.keydownHandler);
    document.addEventListener('keyup', window.keyupHandler);
};