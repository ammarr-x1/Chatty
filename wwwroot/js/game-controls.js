// wwwroot/js/game.js

window.setupKeyboardControls = (dotnetHelper) => {
    // Track which keys are currently pressed to prevent repeating
    const pressedKeys = new Set();

    // Helper to check if user is typing in an input
    const isTyping = () => {
        const el = document.activeElement;
        return el && (el.tagName === 'INPUT' || el.tagName === 'TEXTAREA' || el.isContentEditable);
    };

    window.keydownHandler = async (event) => {
        if (isTyping()) return;

        const key = event.key.toLowerCase();

        // Ignore if key is already pressed (prevents repeat)
        if (pressedKeys.has(key)) {
            return;
        }

        let direction = null;

        switch (key) {
            case 'w': case 'arrowup':
                direction = 'up';
                break;
            case 's': case 'arrowdown':
                direction = 'down';
                break;
            case 'a': case 'arrowleft':
                direction = 'left';
                break;
            case 'd': case 'arrowright':
                direction = 'right';
                break;
        }

        if (direction) {
            event.preventDefault(); // Only prevent default if we're actually using the key for the game
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

    // Remove existing if any (safety)
    document.removeEventListener('keydown', window.keydownHandler);
    document.removeEventListener('keyup', window.keyupHandler);

    document.addEventListener('keydown', window.keydownHandler);
    document.addEventListener('keyup', window.keyupHandler);
};

window.cleanupKeyboardControls = () => {
    if (window.keydownHandler) {
        document.removeEventListener('keydown', window.keydownHandler);
        window.keydownHandler = null;
    }
    if (window.keyupHandler) {
        document.removeEventListener('keyup', window.keyupHandler);
        window.keyupHandler = null;
    }
    console.log('Keyboard controls cleaned up');
};