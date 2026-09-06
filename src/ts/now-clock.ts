const TIME_ZONE = 'Europe/London';

/**
 * Initializes the local-time clock on the /now page, ticking every second.
 */
export function initNowClock(): void {
    const clock = document.querySelector<HTMLElement>('#now-clock');
    if (!clock) {
        return;
    }

    const formatter = new Intl.DateTimeFormat('en-GB', {
        timeZone: TIME_ZONE,
        hour: '2-digit',
        minute: '2-digit',
        second: '2-digit',
        hour12: false,
        timeZoneName: 'short',
    });

    const tick = (): void => {
        clock.textContent = formatter.format(new Date());
    };

    tick();
    setInterval(tick, 1000);
}
