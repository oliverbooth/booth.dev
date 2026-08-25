/**
 * Formats a timestamp into a human-readable relative time string.
 * @param timestamp The timestamp to format.
 * @returns A string representing the relative time (e.g., "5 minutes ago", "in 2 hours").
 */
export function formatRelativeTimestamp(timestamp: Date): string {
    const now = new Date();
    const diff: number = now.getTime() - timestamp.getTime();
    const suffix: string = diff < 0 ? 'from now' : 'ago';

    const seconds: number = Math.floor(diff / 1000);
    if (seconds < 60) {
        return `${seconds} second${seconds !== 1 ? 's' : ''} ${suffix}`;
    }

    const minutes: number = Math.floor(diff / 60000);
    if (minutes < 60) {
        return `${minutes} minute${minutes !== 1 ? 's' : ''} ${suffix}`;
    }

    const hours: number = Math.floor(diff / 3600000);
    if (hours < 24) {
        return `${hours} hour${hours !== 1 ? 's' : ''} ${suffix}`;
    }

    const days: number = Math.floor(diff / 86400000);
    if (days < 30) {
        return `${days} day${days !== 1 ? 's' : ''} ${suffix}`;
    }

    const months: number = Math.floor(diff / 2592000000);
    if (months < 12) {
        return `${months} month${months !== 1 ? 's' : ''} ${suffix}`;
    }

    const years: number = Math.floor(diff / 31536000000);
    return `${years} year${years !== 1 ? 's' : ''} ${suffix}`;
}

/**
 * Converts a base64url string (WebAuthn's on-the-wire encoding for binary fields) to an ArrayBuffer.
 * @param base64url The base64url string to convert.
 * @returns The decoded ArrayBuffer.
 */
export function base64UrlToBuffer(base64url: string): ArrayBuffer {
    const base64 = base64url.replace(/-/g, '+').replace(/_/g, '/');
    const padding = (4 - (base64.length % 4)) % 4;
    const binary = atob(base64 + '='.repeat(padding));
    const bytes = new Uint8Array(binary.length);
    for (let i = 0; i < binary.length; i++) {
        bytes[i] = binary.charCodeAt(i);
    }
    return bytes.buffer;
}

/**
 * Converts an ArrayBuffer to a base64url string, the inverse of {@link base64UrlToBuffer}.
 * @param buffer The ArrayBuffer to convert.
 * @returns The base64url-encoded string.
 */
export function bufferToBase64Url(buffer: ArrayBuffer): string {
    const bytes = new Uint8Array(buffer);
    let binary = '';
    for (const byte of bytes) {
        binary += String.fromCharCode(byte);
    }
    return btoa(binary).replace(/\+/g, '-').replace(/\//g, '_').replace(/=+$/, '');
}

/**
 * Converts ANSI color codes in a string to HTML span elements with inline styles.
 * @param input The input string containing ANSI color codes.
 * @returns The input string with ANSI color codes replaced by HTML span elements.
 */
export function ansiToHtml(input: string): string {
    const ansiColorMap: { [key: string]: string } = {
        '0': 'unset',
        '30': '#0c0c0c',
        '31': '#c50f1f',
        '32': '#13a10e',
        '33': '#c19c00',
        '34': '#0037da',
        '35': '#881798',
        '36': '#3a96dd',
        '37': '#cccccc',
        '90': '#767676'
    };

    let wasOpen: boolean = false;
    return input
        .replace(/\x1b\[(\d+?)m/g, (_, code) => {
            if (code == '0') {
                return '</span>';
            }

            const color: string = ansiColorMap[code];
            const prefix: string = wasOpen ? '</span>' : '';

            if (wasOpen) {
                wasOpen = false;
            }
            if (color) {
                wasOpen = true;
            }

            return color ? `${prefix}<span style="color:${color};">` : '</span>';
        })
        .concat('</span>') // close any open tags at the end
        .replace(/<\/span>(?=<\/span>)/g, ''); // remove redundant closing tags
}
