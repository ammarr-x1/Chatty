// Game keyboard controls for Pacman multiplayer

window.setupKeyboardControls = (dotNetHelper) => {
    const keyHandler = (event) => {
        let direction = null;

        // Map keys to directions
        switch (event.key.toLowerCase()) {
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

        // Send direction to Blazor component
        if (direction) {
            dotNetHelper.invokeMethodAsync('HandleKeyPress', direction);
        }
    };

    // Remove existing listener if any
    if (window.gameKeyHandler) {
        document.removeEventListener('keydown', window.gameKeyHandler);
    }

    // Add new listener
    window.gameKeyHandler = keyHandler;
    document.addEventListener('keydown', keyHandler);

    console.log('Keyboard controls initialized');
};

// Cleanup function
window.cleanupKeyboardControls = () => {
    if (window.gameKeyHandler) {
        document.removeEventListener('keydown', window.gameKeyHandler);
        window.gameKeyHandler = null;
        console.log('Keyboard controls cleaned up');
    }
};
