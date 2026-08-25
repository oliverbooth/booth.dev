/**
 * Initializes the user authoring interface: disabling the password field when "Disable login" is checked.
 * A disabled field is exempt from the browser's own required-field validation, so this also backs the "required
 * on create, unless login is disabled" rule without any extra validation logic.
 */
export function initUserAuthoring(): void {
    const disableLoginCheckbox = document.querySelector<HTMLInputElement>('#disable-login');
    const passwordInput = document.querySelector<HTMLInputElement>('#password');

    if (!disableLoginCheckbox || !passwordInput) {
        return;
    }

    const syncPasswordField = (): void => {
        passwordInput.disabled = disableLoginCheckbox.checked;
        if (disableLoginCheckbox.checked) {
            passwordInput.value = '';
        }
    };

    disableLoginCheckbox.addEventListener('change', syncPasswordField);
    syncPasswordField();
}
