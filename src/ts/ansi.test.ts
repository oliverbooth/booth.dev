import {describe, expect, it} from 'vitest';
import {ansiToHtml} from './utils.ts';

describe('ansiToHtml', () => {
    it('maps the normal foreground colours 30-37 to palette entries 0-7', () => {
        expect(ansiToHtml('\x1b[30mA\x1b[0m')).toBe('<span class="ansi-0">A</span>');
        expect(ansiToHtml('\x1b[31mA\x1b[0m')).toBe('<span class="ansi-1">A</span>');
        expect(ansiToHtml('\x1b[37mA\x1b[0m')).toBe('<span class="ansi-7">A</span>');
    });

    it('maps the bright foreground colours 90-97 to palette entries 8-15', () => {
        expect(ansiToHtml('\x1b[90mA\x1b[0m')).toBe('<span class="ansi-8">A</span>');
        expect(ansiToHtml('\x1b[92mA\x1b[0m')).toBe('<span class="ansi-10">A</span>');
        expect(ansiToHtml('\x1b[97mA\x1b[0m')).toBe('<span class="ansi-15">A</span>');
    });

    it('switches colour without leaving a span open', () => {
        expect(ansiToHtml('\x1b[31mA\x1b[32mB\x1b[0m')).toBe('<span class="ansi-1">A</span><span class="ansi-2">B</span>');
    });

    it('closes a span that is still open at the end of the input', () => {
        expect(ansiToHtml('\x1b[33mA')).toBe('<span class="ansi-3">A</span>');
    });
});
