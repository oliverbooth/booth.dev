import {backward, forward, SpecBound, SpecRange} from './spec-bound';

/**
 * Represents a line specification in the trivia syntax, consisting of a start bound and an optional end bound.
 */
export interface LineSpec {
    /**
     * The starting bound of the line specification.
     */
    readonly start: SpecBound;

    /**
     * The optional ending bound of the line specification. If null, it indicates a single line.
     */
    readonly end: SpecBound | null;
}

/**
 * Represents a highlight token.
 */
export interface HighlightToken {
    /**
     * The line specifications associated with this highlight token. Each line specification defines a range of lines to be highlighted.
     */
    readonly lines: readonly LineSpec[];

    /**
     * Indicates whether the line specifications are grouped together. If true, the lines are treated as a single unit; if false, they are
     * treated as separate units.
     */
    readonly isGrouped: boolean;

    /**
     * The column specifications associated with this highlight token.
     */
    readonly columns: readonly SpecRange[] | null;
}

/**
 * Represents a trivia block.
 */
export interface TriviaBlock {
    /**
     * The kind of the trivia block.
     */
    readonly kind: string;

    /**
     * The highlight tokens associated with this trivia block. Each token defines a set of lines and columns to be highlighted.
     */
    readonly tokens: readonly HighlightToken[];
}

/**
 * Represents the possible errors that can occur during the parsing of trivia syntax.
 */
export type TriviaParseError =
    | 'none'
    | 'emptySpec'
    | 'unexpectedWhitespace'
    | 'invalidNumber'
    | 'invertedRange'
    | 'malformed';

/**
 * Represents the result of parsing trivia syntax.
 */
export type TriviaParseResult =
    | { success: true; block: TriviaBlock }
    | { success: false; error: TriviaParseError };

/**
 * Parses the given input string into a trivia block.
 * @param input The input string to parse.
 * @returns The result of the parsing operation.
 */
export function parseTrivia(input: string): TriviaParseResult {
    if (input.length === 0) {
        return {success: false, error: 'emptySpec'};
    }

    if (/\s/.test(input)) {
        return {success: false, error: 'unexpectedWhitespace'};
    }

    const equalsIndex = input.indexOf('=');
    if (equalsIndex <= 0) {
        return {success: false, error: 'malformed'};
    }

    if (equalsIndex === input.length - 1) {
        return {success: false, error: 'emptySpec'};
    }

    const kind = input.slice(0, equalsIndex);
    const remainder = input.slice(equalsIndex + 1);

    const tokens: HighlightToken[] = [];
    let start = 0;

    while (start < remainder.length) {
        const end = findTokenEnd(remainder, start);
        const tokenResult = parseToken(remainder.slice(start, end));

        if (!tokenResult.success) {
            return tokenResult;
        }

        tokens.push(tokenResult.token);
        start = end + 1;
    }

    if (tokens.length === 0) {
        return {success: false, error: 'emptySpec'};
    }

    return {success: true, block: {kind, tokens}};
}

function findTokenEnd(text: string, start: number): number {
    let depth = 0;

    for (let i = start; i < text.length; i++) {
        const ch = text[i];
        if (ch === '(') {
            depth++;
        } else if (ch === ')') {
            depth--;
        } else if (ch === ',' && depth === 0) {
            return i;
        }
    }

    return text.length;
}

type TokenResult = { success: true; token: HighlightToken } | { success: false; error: TriviaParseError };
type LineGroupResult =
    | { success: true; lines: readonly LineSpec[]; isGrouped: boolean }
    | { success: false; error: TriviaParseError };
type LineResult = { success: true; line: LineSpec } | { success: false; error: TriviaParseError };
type ColumnsResult =
    | { success: true; columns: readonly SpecRange[] }
    | { success: false; error: TriviaParseError };
type RangeResult = { success: true; range: SpecRange } | { success: false; error: TriviaParseError };
type BoundResult = { success: true; bound: SpecBound } | { success: false; error: TriviaParseError };

function parseToken(text: string): TokenResult {
    const atIndex = text.indexOf('@');
    const lineText = atIndex >= 0 ? text.slice(0, atIndex) : text;
    const columnText = atIndex >= 0 ? text.slice(atIndex + 1) : '';

    const groupResult = parseLineGroup(lineText);
    if (!groupResult.success) {
        return groupResult;
    }

    if (atIndex < 0) {
        return {
            success: true,
            token: {lines: groupResult.lines, isGrouped: groupResult.isGrouped, columns: null}
        };
    }

    const columnsResult = parseColumnSpec(columnText);
    if (!columnsResult.success) {
        return columnsResult;
    }

    return {
        success: true,
        token: {lines: groupResult.lines, isGrouped: groupResult.isGrouped, columns: columnsResult.columns}
    };
}

function parseLineGroup(text: string): LineGroupResult {
    let span = text;
    const isGrouped = span.length >= 2 && span[0] === '(' && span[span.length - 1] === ')';
    if (isGrouped) {
        span = span.slice(1, -1);
    }

    const lines: LineSpec[] = [];
    let start = 0;

    while (start < span.length) {
        const commaIndex = span.indexOf(',', start);
        const end = commaIndex < 0 ? span.length : commaIndex;

        const lineResult = parseLineSpec(span.slice(start, end));
        if (!lineResult.success) {
            return lineResult;
        }

        lines.push(lineResult.line);
        start = end + 1;
    }

    if (lines.length === 0) {
        return {success: false, error: 'malformed'};
    }

    // an unparenthesized comma-separated list isn't valid here - a bare 'L1,L2' is split into two
    // separate tokens at the block level before this function ever runs, so this only triggers on
    // a malformed nested construction
    if (!isGrouped && lines.length > 1) {
        return {success: false, error: 'malformed'};
    }

    return {success: true, lines, isGrouped};
}

function parseLineSpec(text: string): LineResult {
    if (text.length === 0 || text[0] !== 'L') {
        return {success: false, error: 'malformed'};
    }

    const span = text.slice(1);

    const dashIndex = span.indexOf('-');
    if (dashIndex < 0) {
        const boundResult = parseBound(span);
        if (!boundResult.success) {
            return boundResult;
        }

        return {success: true, line: {start: boundResult.bound, end: null}};
    }

    const startText = span.slice(0, dashIndex);
    let endText = span.slice(dashIndex + 1);

    if (endText.length === 0 || endText[0] !== 'L') {
        return {success: false, error: 'malformed'};
    }

    endText = endText.slice(1);

    const startResult = parseBound(startText);
    if (!startResult.success) {
        return startResult;
    }

    const endResult = parseBound(endText);
    if (!endResult.success) {
        return endResult;
    }

    if (!startResult.bound.isFromEnd && !endResult.bound.isFromEnd && endResult.bound.value < startResult.bound.value) {
        return {success: false, error: 'invertedRange'};
    }

    return {success: true, line: {start: startResult.bound, end: endResult.bound}};
}

function parseColumnSpec(text: string): ColumnsResult {
    let span = text;
    const isGrouped = span.length >= 2 && span[0] === '(' && span[span.length - 1] === ')';
    if (isGrouped) {
        span = span.slice(1, -1);
    }

    const ranges: SpecRange[] = [];
    let start = 0;

    while (start < span.length) {
        const commaIndex = span.indexOf(',', start);
        const end = commaIndex < 0 ? span.length : commaIndex;

        const rangeResult = parseRange(span.slice(start, end));
        if (!rangeResult.success) {
            return rangeResult;
        }

        ranges.push(rangeResult.range);
        start = end + 1;
    }

    if (ranges.length === 0) {
        return {success: false, error: 'malformed'};
    }

    return {success: true, columns: ranges};
}

function parseRange(text: string): RangeResult {
    const dotDotIndex = text.indexOf('..');
    if (dotDotIndex < 0) {
        return {success: false, error: 'malformed'};
    }

    const startResult = parseBound(text.slice(0, dotDotIndex));
    if (!startResult.success) {
        return startResult;
    }

    const endResult = parseBound(text.slice(dotDotIndex + 2));
    if (!endResult.success) {
        return endResult;
    }

    if (!startResult.bound.isFromEnd && !endResult.bound.isFromEnd && endResult.bound.value < startResult.bound.value) {
        return {success: false, error: 'invertedRange'};
    }

    return {success: true, range: {start: startResult.bound, end: endResult.bound}};
}

function parseBound(text: string): BoundResult {
    let span = text;
    const isFromEnd = span.length > 0 && span[0] === '^';
    if (isFromEnd) {
        span = span.slice(1);
    }

    if (!/^\d+$/.test(span)) {
        return {success: false, error: 'invalidNumber'};
    }

    const value = Number.parseInt(span, 10);
    if (value <= 0) {
        return {success: false, error: 'invalidNumber'};
    }

    return {success: true, bound: isFromEnd ? backward(value) : forward(value)};
}
