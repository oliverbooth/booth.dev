import {EditorView} from 'codemirror';
import {defaultKeymap, history, historyKeymap} from '@codemirror/commands';
import {markdown} from '@codemirror/lang-markdown';
import {languages} from '@codemirror/language-data';
import {syntaxHighlighting, defaultHighlightStyle, HighlightStyle} from '@codemirror/language';
import {keymap} from '@codemirror/view';
import {tags} from '@lezer/highlight';

const highlightStyle = HighlightStyle.define([
    {tag: tags.heading, fontWeight: 'bold', color: 'var(--accent)'},
    {tag: tags.strong, fontWeight: 'bold'},
    {tag: tags.emphasis, fontStyle: 'italic'},
    {tag: tags.monospace, fontFamily: 'var(--font-mono)', fontSize: '0.85em'},
    {tag: tags.link, color: 'var(--accent)', textDecoration: 'underline'},
    {tag: tags.angleBracket, color: 'var(--text-muted)'},
    {tag: tags.comment, color: 'var(--prism-comment)'},
    {tag: tags.string, color: 'var(--prism-string)'},
    {tag: [tags.function(tags.variableName), tags.function(tags.propertyName)], color: 'var(--prism-function)'},
    {tag: tags.className, color: 'var(--prism-class-name)'},
    {tag: [tags.keyword, tags.controlKeyword, tags.operatorKeyword], color: 'var(--prism-keyword)'},
    {tag: tags.punctuation, color: 'var(--prism-foreground)'},
    {tag: tags.tagName, color: 'var(--prism-markup-tag)'},
    {tag: tags.attributeName, color: 'var(--prism-attr-name)'},
    {tag: tags.attributeValue, color: 'var(--prism-markup-attr-value)'},
    {tag: tags.propertyName, color: 'var(--prism-css-property)'},
]);

/**
 * Initializes Markdown editors for all textareas with the `data-markdown` attribute.
 */
export function initMarkdownEditors(): void {
    document.querySelectorAll<HTMLTextAreaElement>('textarea[data-markdown]').forEach((textarea) => {
        const lineHeightPx: number = 14 * 1.6;
        const rows: number = textarea.rows || 10;
        const maxHeight: string = `${rows * lineHeightPx}px`;

        const viewTheme = EditorView.theme({
            '&': {
                color: 'var(--text-primary)',
                fontSize: '14px',
                lineHeight: '1.8',
            },
            '.cm-content': {
                fontFamily: 'var(--font-mono)',
                padding: '0.75rem',
                caretColor: 'var(--text-primary)',
            },
            '.cm-scroller': {overflow: 'auto'},
            '.cm-cursor, .cm-dropCursor': {
                borderLeftColor: 'var(--text-primary)',
                borderLeftWidth: '2px',
            },
            '&.cm-editor': {
                maxHeight,
                background: 'var(--surface-1)',
                border: '0.5px solid var(--border)',
                borderRadius: '8px',
            },
        }, {dark: true});

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
                viewTheme,
                EditorView.lineWrapping,
                EditorView.updateListener.of((update) => {
                    if (update.docChanged) {
                        textarea.value = update.state.doc.toString();
                        textarea.dispatchEvent(new Event('input', {bubbles: true}));
                    }
                }),
            ],
        });

        textarea.style.display = 'none';
        textarea.insertAdjacentElement('afterend', view.dom);
    });
}
