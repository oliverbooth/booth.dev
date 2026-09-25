/**
 * Formats a timestamp into a human-readable relative time string, matching the wording and thresholds of the
 * server's `DateTimeOffset.Humanize()` call in `TimestampRenderer` exactly, so a `<t:R>` timestamp's once-a-second
 * client-side refresh doesn't visibly change the phrasing a reader already saw in the server-rendered HTML.
 *
 * This is a port of Humanizer's default en `DefaultHumanize` algorithm (see `DateTimeHumanizeAlgorithms` in
 * Humanizer.Core), not a from-scratch design - the thresholds and one-of-a-kind phrases ("a minute ago", "yesterday",
 * "one year ago", ...) all mirror it deliberately. If that call in `TimestampRenderer` ever changes, this needs the
 * matching change too.
 * @param timestamp The timestamp to format.
 * @param now The instant to measure `timestamp` against. Defaults to the current time; overridable for testing.
 * @returns A string representing the relative time (e.g., "5 minutes ago", "an hour ago", "yesterday").
 */
export function formatRelativeTimestamp(timestamp: Date, now: Date = new Date()): string {
    const diffMs = now.getTime() - timestamp.getTime();
    const future = diffMs < 0;
    const totalMs = Math.abs(diffMs);

    // "now" isn't a <1000ms special case - it's what a 0-second count naturally formats as. Below one full second,
    // the seconds bucket below computes floor(totalSeconds) = 0, and Humanizer's zero-count phrase is "now"
    if (totalMs < 1000) {
        return 'now';
    }

    const suffix = future ? 'from now' : 'ago';
    const totalSeconds = totalMs / 1000;
    const totalMinutes = totalSeconds / 60;
    const totalHours = totalMinutes / 60;
    const totalDays = totalHours / 24;

    // count === 1 gets Humanizer's hardcoded word instead of "1 <unit>s"; anything else is a plain numeral
    const phrase = (count: number, unitPlural: string, singular: string): string =>
        count === 1 ? `${singular} ${suffix}` : `${count} ${unitPlural} ${suffix}`;

    if (totalSeconds < 60) {
        return phrase(Math.floor(totalSeconds), 'seconds', 'one second');
    }
    if (totalSeconds < 120) {
        return phrase(1, 'minutes', 'a minute');
    }
    if (totalMinutes < 60) {
        return phrase(Math.floor(totalMinutes), 'minutes', 'a minute');
    }
    if (totalMinutes < 90) {
        return phrase(1, 'hours', 'an hour');
    }
    if (totalHours < 24) {
        return phrase(Math.floor(totalHours), 'hours', 'an hour');
    }

    // from here on, "day" counts are a calendar-date difference (UTC, matching the server's use of
    // DateTimeOffset.UtcDateTime), not elapsed time - otherwise a timestamp from just after midnight could read
    // "23 hours ago" server-side but "yesterday" client-side an hour later, or vice versa
    const dayWord = future ? 'tomorrow' : 'yesterday';

    if (totalHours < 48) {
        const days = calendarDaysBetween(timestamp, now);
        return days === 1 ? dayWord : phrase(days, 'days', dayWord);
    }
    if (totalDays < 28) {
        return phrase(Math.floor(totalDays), 'days', dayWord);
    }
    if (totalDays < 30) {
        return isExactlyOneCalendarMonthApart(timestamp, now, future)
            ? phrase(1, 'months', 'one month')
            : phrase(Math.floor(totalDays), 'days', dayWord);
    }
    if (totalDays < 345) {
        return phrase(Math.floor(totalDays / 29.5), 'months', 'one month');
    }

    return phrase(Math.floor(totalDays / 365) || 1, 'years', 'one year');
}

/**
 * The whole number of UTC calendar days between two instants (e.g. 23:59 and 00:01 the next day are 1 day apart,
 * even though only 2 minutes elapsed) - Humanizer's "yesterday"/"tomorrow" cutoff is calendar-based, not elapsed-time.
 */
function calendarDaysBetween(a: Date, b: Date): number {
    const aUtc = Date.UTC(a.getUTCFullYear(), a.getUTCMonth(), a.getUTCDate());
    const bUtc = Date.UTC(b.getUTCFullYear(), b.getUTCMonth(), b.getUTCDate());
    return Math.round(Math.abs(aUtc - bUtc) / 86400000);
}

/**
 * Whether `timestamp` falls exactly one calendar month before (or after, if `future`) `now`'s date - e.g. now =
 * March 15th, timestamp = February 15th. Used only to disambiguate the 28-29 day range, where Humanizer says
 * "one month ago" for an exact month-to-date match and otherwise falls back to a day count.
 */
function isExactlyOneCalendarMonthApart(timestamp: Date, now: Date, future: boolean): boolean {
    const reference = new Date(Date.UTC(now.getUTCFullYear(), now.getUTCMonth() + (future ? 1 : -1), now.getUTCDate()));
    return reference.getUTCFullYear() === timestamp.getUTCFullYear()
        && reference.getUTCMonth() === timestamp.getUTCMonth()
        && reference.getUTCDate() === timestamp.getUTCDate();
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
 * Maps an SGR foreground colour code to the index of its entry in the 16-colour terminal palette.
 * @param code The SGR code: 30-37 for the normal colours, 90-97 for the bright ones.
 * @returns The palette index (0-15), or `null` if the code is not a foreground colour.
 */
function ansiPaletteIndex(code: string): number | null {
    const value: number = Number(code);
    if (value >= 30 && value <= 37) {
        return value - 30;
    }

    if (value >= 90 && value <= 97) {
        return value - 90 + 8;
    }

    return null;
}

/**
 * Converts ANSI color codes in a string to HTML span elements carrying an `ansi-N` class, which the stylesheet
 * resolves against the theme's 16-colour terminal palette.
 * @param input The input string containing ANSI color codes.
 * @returns The input string with ANSI color codes replaced by HTML span elements.
 */
export function ansiToHtml(input: string): string {
    let wasOpen: boolean = false;
    return input
        .replace(/\x1b\[(\d+?)m/g, (_, code) => {
            if (code == '0') {
                return '</span>';
            }

            const index: number | null = ansiPaletteIndex(code);
            const prefix: string = wasOpen ? '</span>' : '';

            if (wasOpen) {
                wasOpen = false;
            }
            if (index !== null) {
                wasOpen = true;
            }

            return index !== null ? `${prefix}<span class="ansi-${index}">` : '</span>';
        })
        .concat('</span>')
        .replace(/<\/span>(?=<\/span>)/g, '');
}
