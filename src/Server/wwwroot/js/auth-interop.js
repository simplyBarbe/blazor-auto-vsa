// Auth interop functions for Blazor
// Cookie operations MUST go through the browser's fetch API (not SignalR)
window.authInterop = {
    login: async function (email, password, rememberMe) {
        try {
            const response = await fetch('/api/auth/login', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ email, password, rememberMe }),
                credentials: 'include'
            });

            if (response.ok) {
                const userInfo = await response.json();
                return { succeeded: true, userInfo: userInfo, errorMessage: null };
            } else {
                return { succeeded: false, userInfo: null, errorMessage: 'Invalid email or password.' };
            }
        } catch (error) {
            return { succeeded: false, userInfo: null, errorMessage: error.message };
        }
    },

    logout: async function () {
        try {
            await fetch('/api/auth/logout', {
                method: 'POST',
                credentials: 'include'
            });
            return true;
        } catch {
            return false;
        }
    }
};
