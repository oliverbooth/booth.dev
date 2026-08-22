import {base64UrlToBuffer, bufferToBase64Url} from '../utils.ts';

/**
 * Initializes passkey registration/management on the admin user editor: registering a new passkey via
 * `navigator.credentials.create`. Deletion is a plain server round-trip and needs no JS.
 */
export function initPasskeyRegistration(): void {
    const form = document.querySelector<HTMLFormElement>('.form-grid');
    const registerButton = document.querySelector<HTMLButtonElement>('#passkey-register-btn');
    const nicknameInput = document.querySelector<HTMLInputElement>('#passkey-nickname');
    const errorBox = document.querySelector<HTMLElement>('#passkey-error');

    if (!form || !registerButton || !nicknameInput || !errorBox) {
        return;
    }

    registerButton.addEventListener('click', () => {
        void registerPasskey(form, registerButton, errorBox);
    });
}

/**
 * Runs the full passkey registration ceremony: fetches creation options from the server, prompts the
 * authenticator via the browser's WebAuthn API, then posts the result back to complete registration.
 */
async function registerPasskey(form: HTMLFormElement, button: HTMLButtonElement, errorBox: HTMLElement): Promise<void> {
    errorBox.hidden = true;
    button.disabled = true;

    try {
        const beginUrl = new URL(form.action);
        beginUrl.searchParams.set('handler', 'BeginPasskeyRegistration');
        const beginResponse = await fetch(beginUrl, {method: 'POST', body: new FormData(form)});

        if (!beginResponse.ok) {
            throw new Error('Could not start passkey registration.');
        }

        const options = await beginResponse.json();

        const credential = await navigator.credentials.create({
            publicKey: {
                ...options,
                challenge: base64UrlToBuffer(options.challenge),
                user: {...options.user, id: base64UrlToBuffer(options.user.id)},
                excludeCredentials: (options.excludeCredentials ?? []).map((descriptor: { id: string }) => ({
                    ...descriptor,
                    id: base64UrlToBuffer(descriptor.id),
                })),
            },
        }) as PublicKeyCredential;

        const response = credential.response as AuthenticatorAttestationResponse;
        const credentialJson = JSON.stringify({
            id: credential.id,
            rawId: bufferToBase64Url(credential.rawId),
            type: credential.type,
            response: {
                attestationObject: bufferToBase64Url(response.attestationObject),
                clientDataJSON: bufferToBase64Url(response.clientDataJSON),
            },
        });

        const completeUrl = new URL(form.action);
        completeUrl.searchParams.set('handler', 'CompletePasskeyRegistration');
        const formData = new FormData(form);
        formData.set('credentialJson', credentialJson);

        const completeResponse = await fetch(completeUrl, {method: 'POST', body: formData});
        const result = await completeResponse.json() as { success: boolean; error?: string };

        if (!result.success) {
            throw new Error(result.error ?? 'Passkey registration failed.');
        }

        window.location.reload();
    } catch (error) {
        errorBox.textContent = error instanceof Error ? error.message : 'Passkey registration failed.';
        errorBox.hidden = false;
        button.disabled = false;
    }
}
