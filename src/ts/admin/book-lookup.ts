interface BookLookupCandidate {
    title: string;
    author: string;
    isbn: string;
}

interface BookLookupResponse {
    candidates?: BookLookupCandidate[];
    error?: string;
}

/**
 * Initializes the title/ISBN lookup on the "new book" form. Looking up a query fills the Title, Author, and ISBN
 * fields (marked `data-book-field`) directly when it resolves to exactly one match, or lists candidates to pick
 * from otherwise; the fields stay editable either way, since the lookup is just a shortcut, not a requirement.
 */
export function initBookLookup(): void {
    const form = document.querySelector<HTMLFormElement>('.form-grid');
    const queryInput = document.querySelector<HTMLInputElement>('#lookup-query');
    const lookupButton = document.querySelector<HTMLButtonElement>('#lookup-btn');
    const results = document.querySelector<HTMLUListElement>('#lookup-results');
    const hint = document.querySelector<HTMLElement>('#lookup-hint');
    const titleField = document.querySelector<HTMLInputElement>('[data-book-field="title"]');
    const authorField = document.querySelector<HTMLInputElement>('[data-book-field="author"]');
    const isbnField = document.querySelector<HTMLInputElement>('[data-book-field="isbn"]');

    if (!form || !queryInput || !lookupButton || !results || !hint || !titleField || !authorField || !isbnField) {
        return;
    }

    const applyCandidate = (candidate: BookLookupCandidate): void => {
        titleField.value = candidate.title;
        authorField.value = candidate.author;
        isbnField.value = candidate.isbn;
        results.hidden = true;
        results.innerHTML = '';
        hint.hidden = true;
    };

    const showCandidates = (candidates: BookLookupCandidate[]): void => {
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
            meta.textContent = `${candidate.author} · ${candidate.isbn}`;

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

            const payload = await response.json() as BookLookupResponse;
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
