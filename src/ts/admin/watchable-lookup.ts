interface WatchableLookupCandidate {
    title: string;
    kind: string;
    year: number | null;
}

interface WatchableLookupResponse {
    candidates?: WatchableLookupCandidate[];
    error?: string;
}

/**
 * Initializes the TMDB title lookup on the "new watchlist entry" form.
 */
export function initWatchableLookup(): void {
    const form = document.querySelector<HTMLFormElement>('.form-grid');
    const queryInput = document.querySelector<HTMLInputElement>('#lookup-query');
    const lookupButton = document.querySelector<HTMLButtonElement>('#lookup-btn');
    const results = document.querySelector<HTMLUListElement>('#lookup-results');
    const hint = document.querySelector<HTMLElement>('#lookup-hint');
    const titleField = document.querySelector<HTMLInputElement>('[data-watchable-field="title"]');
    const kindField = document.querySelector<HTMLSelectElement>('[data-watchable-field="kind"]');

    if (!form || !queryInput || !lookupButton || !results || !hint || !titleField || !kindField) {
        return;
    }

    const applyCandidate = (candidate: WatchableLookupCandidate): void => {
        titleField.value = candidate.title;
        kindField.value = candidate.kind;
        results.hidden = true;
        results.innerHTML = '';
        hint.hidden = true;
    };

    const showCandidates = (candidates: WatchableLookupCandidate[]): void => {
        results.innerHTML = '';

        if (candidates.length === 1) {
            applyCandidate(candidates[0]);
            return;
        }

        for (const candidate of candidates) {
            const item = document.createElement('li');
            item.className = 'draft-item';

            const button = document.createElement('button');
            button.type = 'button';
            button.className = 'lookup-result';
            button.addEventListener('click', () => applyCandidate(candidate));

            const body = document.createElement('div');
            body.className = 'draft-item-body';

            const name = document.createElement('p');
            name.className = 'name';
            name.textContent = candidate.title;

            const meta = document.createElement('p');
            meta.className = 'meta';
            meta.textContent = `${candidate.kind} · ${candidate.year ?? 'unknown year'}`;

            body.append(name, meta);
            button.append(body);
            item.append(button);
            results.append(item);
        }

        results.hidden = false;
        hint.hidden = true;
    };

    const showError = (message: string): void => {
        results.hidden = true;
        results.innerHTML = '';
        hint.textContent = message;
        hint.hidden = false;
    };

    const runLookup = async (): Promise<void> => {
        const query = queryInput.value.trim();
        if (!query) {
            return;
        }

        lookupButton.disabled = true;

        try {
            const url = new URL(form.action);
            url.searchParams.set('handler', 'Lookup');

            const formData = new FormData(form);
            formData.set('query', query);

            const response = await fetch(url, {
                method: 'POST',
                body: formData,
                headers: {Accept: 'application/json'},
            });

            const payload = await response.json() as WatchableLookupResponse;
            if (payload.error || !payload.candidates) {
                showError(payload.error ?? "Couldn't look that up. Enter the details by hand instead.");
                return;
            }

            showCandidates(payload.candidates);
        } catch {
            showError("Couldn't look that up. Enter the details by hand instead.");
        } finally {
            lookupButton.disabled = false;
        }
    };

    lookupButton.addEventListener('click', () => void runLookup());
    queryInput.addEventListener('keydown', (event) => {
        if (event.key === 'Enter') {
            event.preventDefault();
            void runLookup();
        }
    });
}
