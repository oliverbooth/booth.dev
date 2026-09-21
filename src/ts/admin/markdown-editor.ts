import {EditorView} from 'codemirror';
import {defaultKeymap, history, historyKeymap} from '@codemirror/commands';
import {markdown} from '@codemirror/lang-markdown';
import {languages} from '@codemirror/language-data';
import {syntaxHighlighting, defaultHighlightStyle, HighlightStyle} from '@codemirror/language';
import {Compartment} from '@codemirror/state';
import {keymap} from '@codemirror/view';
import {tags} from '@lezer/highlight';
import {currentTheme} from '../theme.ts';

const highlightStyle = HighlightStyle.define([
    {tag: tags.heading, fontWeight: 'bold', color: 'var(--brand-text)'},
    {tag: tags.strong, fontWeight: 'bold'},
    {tag: tags.emphasis, fontStyle: 'italic'},
    {tag: tags.monospace, fontFamily: 'var(--font-mono)', fontSize: '0.85em'},
    {tag: tags.link, color: 'var(--brand-text)', textDecoration: 'underline'},
    {tag: tags.angleBracket, color: 'var(--text-faint)'},
    {tag: tags.comment, color: 'var(--md-comment)'},
    {tag: tags.string, color: 'var(--md-string)'},
    {tag: [tags.function(tags.variableName), tags.function(tags.propertyName)], color: 'var(--md-func)'},
    {tag: tags.className, color: 'var(--md-type)'},
    {tag: [tags.keyword, tags.controlKeyword, tags.operatorKeyword], color: 'var(--md-keyword)'},
    {tag: tags.punctuation, color: 'var(--md-punct)'},
    {tag: tags.tagName, color: 'var(--md-keyword)'},
    {tag: tags.attributeName, color: 'var(--md-type)'},
    {tag: tags.attributeValue, color: 'var(--md-string)'},
    {tag: tags.propertyName, color: 'var(--md-type)'},
]);

/**
 * Initializes Markdown editors for all textareas with the `data-markdown` attribute.
 */
export function initMarkdownEditors(): void {
    document.querySelectorAll<HTMLTextAreaElement>('textarea[data-markdown]').forEach((textarea) => {
        void mountEditor(textarea);
    });
}

/**
 * Mounts a single CodeMirror editor onto a textarea, once the page's layout has settled.
 * @param textarea The textarea to mount the editor onto.
 */
async function mountEditor(textarea: HTMLTextAreaElement): Promise<void> {
    await document.fonts.ready;
    await new Promise(requestAnimationFrame);
    await new Promise(requestAnimationFrame);

    const lineHeightPx: number = 14 * 1.6;
    const rows: number = textarea.rows || 10;
    const maxHeight: string = `${rows * lineHeightPx}px`;

    // the editor is styled to match the other form fields (see _forms.css); `dark` only picks CodeMirror's own defaults
    const buildTheme = (dark: boolean) => EditorView.theme({
        '&': {
            color: 'var(--text)',
            fontSize: '14px',
            lineHeight: '1.8',
        },
        '.cm-content': {
            fontFamily: 'var(--font-mono)',
            padding: '11px 14px',
            caretColor: 'var(--text)',
        },
        '.cm-scroller': {overflow: 'auto'},
        '.cm-cursor, .cm-dropCursor': {
            borderLeftColor: 'var(--text)',
            borderLeftWidth: '2px',
        },
        '&.cm-editor': {
            maxHeight,
            background: 'var(--surface)',
            boxShadow: 'inset 0 -2px 0 var(--text-faint)',
            borderRadius: '12px',
            transition: 'background-color 0.15s ease, box-shadow 0.15s ease',
        },
        // CodeMirror's own focus outline is replaced by the thicker brand underline; the transparent outline
        // only shows in forced-colors mode
        '&.cm-editor.cm-focused': {
            outline: '2px solid transparent',
            outlineOffset: '2px',
            background: 'var(--input-focus-bg)',
            boxShadow: 'inset 0 -3px 0 var(--brand-text)',
        },
        '&.cm-editor.cm-focused > .cm-scroller > .cm-selectionLayer .cm-selectionBackground, .cm-selectionBackground, .cm-content ::selection': {
            background: 'color-mix(in oklch, var(--brand) 35%, transparent)',
        },
    }, {dark});
    const themeCompartment = new Compartment();

    const view = new EditorView({
        doc: textarea.value,
        extensions: [
            history(),
            // `markdown()` binds its own Prec.high Enter handler for continuing lists/blockquotes, but its own
            // docs say it must not be the only Enter binding — it deliberately no-ops outside that context and
            // falls through to whatever's next. Without defaultKeymap providing that fallback, "next" was
            // nothing: Enter fell all the way through to unintercepted native contenteditable behavior, which is
            // exactly the kind of thing that gets flaky creating consecutive empty blocks at the end of a
            // document. defaultKeymap's own insertNewlineAndIndent is the real fallback; historyKeymap gives
            // undo/redo, which was equally absent.
            keymap.of([...defaultKeymap, ...historyKeymap]),
            markdown({codeLanguages: languages}),
            syntaxHighlighting(defaultHighlightStyle, {fallback: true}),
            syntaxHighlighting(highlightStyle),
            themeCompartment.of(buildTheme(currentTheme() === 'dark')),
            EditorView.lineWrapping,
            EditorView.updateListener.of((update) => {
                if (update.docChanged) {
                    textarea.value = update.state.doc.toString();
                    textarea.dispatchEvent(new Event('input', {bubbles: true}));
                }
            }),
        ],
    });

    // CodeMirror themes are fixed at mount, so a theme switch has to reconfigure each open editor
    document.addEventListener('themechange', () => {
        view.dispatch({effects: themeCompartment.reconfigure(buildTheme(currentTheme() === 'dark'))});
    });

    textarea.style.display = 'none';
    textarea.insertAdjacentElement('afterend', view.dom);
}
