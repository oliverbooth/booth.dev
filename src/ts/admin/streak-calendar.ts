type CheckInKind = 'Completed' | 'Frozen';
type CalendarAction = CheckInKind | 'Remove';

interface CalendarResponse {
    checkIns: { date: string; kind: CheckInKind }[];
    current: number;
    best: number;
}

const WEEKDAY_OFFSET = 6; // JS getDay() is 0=Sunday; the grid starts on Monday

/**
 * Initializes the streak editor's interactive calendar: a month grid, seeded from the check-ins rendered into
 * `[data-calendar-source]`, where clicking a day (or dragging across several) selects it and shows an action bar to
 * mark the selection completed/frozen or remove it entirely.
 */
export function initStreakCalendar(): void {
    const root = document.querySelector<HTMLElement>('[data-streak-calendar]');
    const grid = root?.querySelector<HTMLElement>('[data-calendar-grid]');
    const monthLabel = root?.querySelector<HTMLButtonElement>('[data-calendar-month-label]');
    const monthInput = root?.querySelector<HTMLInputElement>('[data-calendar-month-input]');
    const prevButton = root?.querySelector<HTMLButtonElement>('[data-calendar-prev]');
    const nextButton = root?.querySelector<HTMLButtonElement>('[data-calendar-next]');
    const todayButton = root?.querySelector<HTMLButtonElement>('[data-calendar-today]');

    if (!root || !grid || !monthLabel || !monthInput || !prevButton || !nextButton || !todayButton) {
        return;
    }

    const streakId = root.dataset.streakId ?? '';
    const logUrl = root.dataset.logUrl ?? '';
    const removeUrl = root.dataset.removeUrl ?? '';
    const bulkSetUrl = root.dataset.bulkSetUrl ?? '';
    const bulkRemoveUrl = root.dataset.bulkRemoveUrl ?? '';
    const token = document.querySelector<HTMLInputElement>('input[name="__RequestVerificationToken"]')?.value ?? '';

    const checkIns = new Map<string, CheckInKind>();
    for (const source of root.querySelectorAll<HTMLElement>('[data-calendar-source] [data-date]')) {
        checkIns.set(source.dataset.date ?? '', (source.dataset.kind as CheckInKind) ?? 'Completed');
    }

    const selectionBar = buildSelectionBar(root, (action) => void applyToSelection(action), clearSelection);

    const today = new Date();
    let viewYear = today.getFullYear();
    let viewMonth = today.getMonth();
    let selectedDates = new Set<string>();
    let dragAnchor: string | null = null;
    let isDragging = false;

    render();

    prevButton.addEventListener('click', () => {
        viewMonth -= 1;
        if (viewMonth < 0) {
            viewMonth = 11;
            viewYear -= 1;
        }

        render();
    });

    nextButton.addEventListener('click', () => {
        viewMonth += 1;
        if (viewMonth > 11) {
            viewMonth = 0;
            viewYear += 1;
        }

        render();
    });

    todayButton.addEventListener('click', () => {
        viewYear = today.getFullYear();
        viewMonth = today.getMonth();
        render();
    });

    monthLabel.addEventListener('click', () => {
        monthInput.value = `${viewYear}-${String(viewMonth + 1).padStart(2, '0')}`;
        monthLabel.hidden = true;
        monthInput.hidden = false;
        monthInput.focus();
        monthInput.showPicker?.();
    });

    monthInput.addEventListener('change', () => {
        const [year, month] = monthInput.value.split('-').map(Number);
        if (year && month) {
            viewYear = year;
            viewMonth = month - 1;
            render();
        }
    });

    monthInput.addEventListener('blur', () => {
        monthInput.hidden = true;
        monthLabel.hidden = false;
    });

    document.addEventListener('pointerup', () => {
        if (isDragging) {
            isDragging = false;
            showSelection();
        }
    });

    function render(): void {
        grid.innerHTML = '';
        monthLabel.textContent = new Date(viewYear, viewMonth, 1).toLocaleString('en-US', {month: 'long', year: 'numeric'});

        const firstWeekday = (new Date(viewYear, viewMonth, 1).getDay() + WEEKDAY_OFFSET) % 7;
        const daysInMonth = new Date(viewYear, viewMonth + 1, 0).getDate();
        const todayKey = dateKey(today.getFullYear(), today.getMonth(), today.getDate());

        for (let i = 0; i < firstWeekday; i++) {
            const filler = document.createElement('span');
            filler.className = 'streak-calendar-cell is-empty';
            grid.appendChild(filler);
        }

        for (let day = 1; day <= daysInMonth; day++) {
            const key = dateKey(viewYear, viewMonth, day);
            const cell = document.createElement('button');
            cell.type = 'button';
            cell.className = 'streak-calendar-cell';
            cell.textContent = String(day);
            cell.dataset.date = key;

            const kind = checkIns.get(key);
            if (kind === 'Completed') {
                cell.classList.add('is-completed');
            } else if (kind === 'Frozen') {
                cell.classList.add('is-frozen');
                cell.appendChild(createSnowflake());
            }

            if (key === todayKey) {
                cell.classList.add('is-today');
            }

            cell.addEventListener('pointerdown', (event) => {
                event.preventDefault();

                if (event.ctrlKey || event.metaKey) {
                    if (selectedDates.has(key)) {
                        selectedDates.delete(key);
                    } else {
                        selectedDates.add(key);
                    }

                    highlightSelection();
                    showSelection();
                    return;
                }

                isDragging = true;
                dragAnchor = key;
                selectedDates = new Set([key]);
                highlightSelection();
            });

            cell.addEventListener('pointerenter', () => {
                if (isDragging && dragAnchor) {
                    selectedDates = rangeBetween(dragAnchor, key);
                    highlightSelection();
                }
            });

            grid.appendChild(cell);
        }

        highlightSelection();
    }

    function highlightSelection(): void {
        for (const cell of grid!.querySelectorAll<HTMLElement>('[data-date]')) {
            cell.classList.toggle('is-selected', selectedDates.has(cell.dataset.date ?? ''));
        }
    }

    function showSelection(): void {
        if (selectedDates.size === 0) {
            selectionBar.hide();
            return;
        }

        selectionBar.show(describeSelection(selectedDates));
    }

    function clearSelection(): void {
        selectedDates = new Set();
        dragAnchor = null;
        selectionBar.hide();
        highlightSelection();
    }

    async function applyToSelection(action: CalendarAction): Promise<void> {
        const dates = [...selectedDates];
        if (dates.length === 0) {
            return;
        }

        const contiguous = dates.length > 1 ? contiguousRange(selectedDates) : null;
        let lastResponse: CalendarResponse | null = null;

        if (contiguous) {
            // A multi-day unbroken range can use the cheaper bulk endpoints - but those only edit days that are
            // already logged (by design, same as the old "bulk edit a range" box), so a single day always goes
            // through the single-day endpoints below instead, which create a day that isn't logged yet.
            const [start, end] = contiguous;
            lastResponse = action === 'Remove'
                ? await send(bulkRemoveUrl, {start, end})
                : await send(bulkSetUrl, {start, end, kind: action});
        } else {
            // A single day, or a discontiguous set - apply one day at a time, sequentially so each response
            // reflects every prior write.
            for (const date of dates) {
                lastResponse = action === 'Remove'
                    ? await send(removeUrl, {date})
                    : await send(logUrl, {date, kind: action});
            }
        }

        if (lastResponse) {
            applyResponse(lastResponse);
        }

        clearSelection();
    }

    async function send(url: string, params: Record<string, string>): Promise<CalendarResponse | null> {
        const body = new URLSearchParams({__RequestVerificationToken: token, id: streakId, ...params});
        const response = await fetch(url, {method: 'POST', headers: {Accept: 'application/json'}, body});
        return response.ok ? (await response.json() as CalendarResponse) : null;
    }

    function applyResponse(data: CalendarResponse): void {
        checkIns.clear();
        for (const checkIn of data.checkIns) {
            checkIns.set(checkIn.date, checkIn.kind);
        }

        updateStats(data.current, data.best);
        render();
    }
}

/**
 * Returns every date key from `a` to `b` inclusive, in either order.
 */
function rangeBetween(a: string, b: string): Set<string> {
    const start = parseDateKey(a);
    const end = parseDateKey(b);
    const [lo, hi] = start <= end ? [start, end] : [end, start];

    const result = new Set<string>();
    for (const cursor = new Date(lo); cursor <= hi; cursor.setDate(cursor.getDate() + 1)) {
        result.add(dateKey(cursor.getFullYear(), cursor.getMonth(), cursor.getDate()));
    }

    return result;
}

/**
 * Returns `[start, end]` if every date in the selection forms one unbroken run of consecutive days, so the caller
 * can use the cheaper bulk-range endpoints instead of one request per day; otherwise returns `null`.
 */
function contiguousRange(dates: Set<string>): [string, string] | null {
    if (dates.size === 0) {
        return null;
    }

    if (dates.size === 1) {
        const only = [...dates][0];
        return [only, only];
    }

    const sorted = [...dates].sort();
    for (let i = 1; i < sorted.length; i++) {
        const previous = parseDateKey(sorted[i - 1]);
        previous.setDate(previous.getDate() + 1);
        if (dateKey(previous.getFullYear(), previous.getMonth(), previous.getDate()) !== sorted[i]) {
            return null;
        }
    }

    return [sorted[0], sorted[sorted.length - 1]];
}

function describeSelection(dates: Set<string>): string {
    const range = contiguousRange(dates);
    if (range) {
        const [start, end] = range;
        return start === end ? start : `${start} → ${end}`;
    }

    return `${dates.size} days selected`;
}

function parseDateKey(key: string): Date {
    const [year, month, day] = key.split('-').map(Number);
    return new Date(year, month - 1, day);
}

function dateKey(year: number, month: number, day: number): string {
    return `${year}-${String(month + 1).padStart(2, '0')}-${String(day).padStart(2, '0')}`;
}

/**
 * Builds the small snowflake marker shown on a frozen day, matching the icon used on the public streaks page.
 */
function createSnowflake(): HTMLElement {
    const icon = document.createElement('i');
    icon.className = 'streak-calendar-snowflake ti ti-snowflake';
    icon.setAttribute('aria-hidden', 'true');
    return icon;
}

/**
 * Builds the below-grid action bar shown once a day (or several) is selected, and returns handles to show or hide
 * it. Kept as a plain bar rather than a floating popover - simpler to position correctly and works the same on
 * touch.
 */
function buildSelectionBar(root: HTMLElement, onAction: (action: CalendarAction) => void, onCancel: () => void): { show: (label: string) => void; hide: () => void } {
    const bar = document.createElement('div');
    bar.className = 'streak-calendar-selection';
    bar.hidden = true;

    const label = document.createElement('span');
    bar.appendChild(label);

    const actions = document.createElement('div');
    actions.className = 'streak-calendar-selection-actions';
    bar.appendChild(actions);

    const completedButton = document.createElement('button');
    completedButton.type = 'button';
    completedButton.className = 'btn';
    completedButton.textContent = 'Mark completed';
    actions.appendChild(completedButton);

    const frozenButton = document.createElement('button');
    frozenButton.type = 'button';
    frozenButton.className = 'btn';
    frozenButton.textContent = 'Mark frozen';
    actions.appendChild(frozenButton);

    const removeButton = document.createElement('button');
    removeButton.type = 'button';
    removeButton.className = 'btn btn-danger';
    removeButton.textContent = 'Remove';
    actions.appendChild(removeButton);

    const cancelButton = document.createElement('button');
    cancelButton.type = 'button';
    cancelButton.className = 'streak-calendar-cancel';
    cancelButton.textContent = 'cancel';
    actions.appendChild(cancelButton);

    const grid = root.querySelector('[data-calendar-grid]');
    grid?.insertAdjacentElement('afterend', bar);

    cancelButton.addEventListener('click', onCancel);

    completedButton.addEventListener('click', () => onAction('Completed'));
    frozenButton.addEventListener('click', () => onAction('Frozen'));
    removeButton.addEventListener('click', () => onAction('Remove'));

    return {
        show(text) {
            label.textContent = text;
            bar.hidden = false;
        },
        hide() {
            bar.hidden = true;
        },
    };
}

function updateStats(current: number, best: number): void {
    const label = document.querySelector<HTMLElement>('[data-streak-stats]');
    if (label) {
        label.textContent = `${current} current · ${best} best`;
    }
}
