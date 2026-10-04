interface PlayableLookupCandidate {
    title: string;
    slug: string;
    year: number | null;
    type: string | null;
}

interface PlayableLookupResponse {
    candidates?: PlayableLookupCandidate[];
    error?: string;
}

/**
 * Initializes the IGDB title lookup on the "new game list entry" form.
 */
export function initPlayableLookup(): void {
    const form = document.querySelector<HTMLFormElement>('.form-grid');
    const queryInput = document.querySelector<HTMLInputElement>('#lookup-query');
    const lookupButton = document.querySelector<HTMLButtonElement>('#lookup-btn');
    const results = document.querySelector<HTMLUListElement>('#lookup-results');
    const hint = document.querySelector<HTMLElement>('#lookup-hint');
    const titleField = document.querySelector<HTMLInputElement>('[data-playable-field="title"]');
    const igdbField = document.querySelector<HTMLInputElement>('[data-playable-field="igdb"]');

    if (!form || !queryInput || !lookupButton || !results || !hint || !titleField || !igdbField) {
        return;
    }

    const applyCandidate = (candidate: PlayableLookupCandidate): void => {
        titleField.value = candidate.title;
        igdbField.value = candidate.slug;
        results.hidden = true;
        results.innerHTML = '';
        hint.hidden = true;
    };

    const showCandidates = (candidates: PlayableLookupCandidate[]): void => {
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
            meta.textContent = String(candidate.year ?? 'unknown year');

            if (candidate.type) {
                const badge = document.createElement('span');
                badge.className = candidate.type === 'Main Game' ? 'badge badge-unlisted' : 'badge badge-brand';
                badge.textContent = candidate.type;
                meta.append(badge);
            }

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

            const payload = await response.json() as PlayableLookupResponse;
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
