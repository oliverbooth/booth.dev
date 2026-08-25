import {base64UrlToBuffer, bufferToBase64Url} from '../utils.ts';

/**
 * Initializes the "Sign in with a passkey" button on the admin login page: a one-tap, usernameless
 * alternative to typing a password and TOTP code, via `navigator.credentials.get`.
 */
export function initPasskeyLogin(): void {
    const form = document.querySelector<HTMLFormElement>('.login-card form');
    const loginButton = document.querySelector<HTMLButtonElement>('#passkey-login-btn');
    const errorBox = document.querySelector<HTMLElement>('#passkey-error');

    if (!form || !loginButton || !errorBox) {
        return;
    }

    loginButton.addEventListener('click', () => {
        void loginWithPasskey(form, loginButton, errorBox);
    });
}

/**
 * Runs the full passkey login ceremony: fetches assertion options from the server, prompts for a passkey
 * via the browser's WebAuthn API, then posts the result back to complete sign-in.
 */
async function loginWithPasskey(form: HTMLFormElement, button: HTMLButtonElement, errorBox: HTMLElement): Promise<void> {
    errorBox.hidden = true;
    button.disabled = true;

    try {
        const beginUrl = new URL(form.action);
        beginUrl.searchParams.set('handler', 'BeginPasskeyLogin');
        const beginResponse = await fetch(beginUrl, {method: 'POST', body: new FormData(form)});

        if (!beginResponse.ok) {
            throw new Error('Could not start passkey login.');
        }

        const options = await beginResponse.json();

        const credential = await navigator.credentials.get({
            publicKey: {
                ...options,
                challenge: base64UrlToBuffer(options.challenge),
                allowCredentials: (options.allowCredentials ?? []).map((descriptor: { id: string }) => ({
                    ...descriptor,
                    id: base64UrlToBuffer(descriptor.id),
                })),
            },
        }) as PublicKeyCredential;

        const response = credential.response as AuthenticatorAssertionResponse;
        const credentialJson = JSON.stringify({
            id: credential.id,
            rawId: bufferToBase64Url(credential.rawId),
            type: credential.type,
            response: {
                authenticatorData: bufferToBase64Url(response.authenticatorData),
                clientDataJSON: bufferToBase64Url(response.clientDataJSON),
                signature: bufferToBase64Url(response.signature),
                userHandle: response.userHandle ? bufferToBase64Url(response.userHandle) : null,
            },
        });

        const completeUrl = new URL(form.action);
        completeUrl.searchParams.set('handler', 'CompletePasskeyLogin');
        const formData = new FormData(form);
        formData.set('credentialJson', credentialJson);

        const completeResponse = await fetch(completeUrl, {method: 'POST', body: formData});
        const result = await completeResponse.json() as { success: boolean; error?: string };

        if (!result.success) {
            throw new Error(result.error ?? 'Passkey login failed.');
        }

        window.location.href = '/admin';
    } catch (error) {
        errorBox.textContent = error instanceof Error ? error.message : 'Passkey login failed.';
        errorBox.hidden = false;
        button.disabled = false;
    }
}
