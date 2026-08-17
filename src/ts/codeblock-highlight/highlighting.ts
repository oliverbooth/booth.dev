import {resolveBound, resolveRange, SpecRange} from './spec-bound';
import {HighlightToken, parseTrivia, TriviaBlock} from './trivia-parser';

/**
 * Represents a range of absolute character offsets.
 */
interface AbsoluteRange {
    /**
     * The starting character offset of the range.
     */
    start: number;

    /**
     * The ending character offset of the range.
     */
    end: number;
}

/**
 * Applies syntax highlighting to a code block element based on the `data-highlight` attribute.
 * @param codeElement The code block element to which to apply highlighting.
 */
export function applyCodeBlockHighlights(codeElement: HTMLElement): void {
    const highlightAttr = codeElement.getAttribute('data-highlight');
    if (highlightAttr === null) {
        return;
    }

    const block = extractHighlightBlock(highlightAttr);
    if (block === null) {
        return;
    }

    const sourceText = (codeElement.textContent ?? '').replace(/\n$/, '');
    const lineOffsets = computeLineOffsets(sourceText);

    const ranges = resolveAbsoluteRanges(block, lineOffsets, sourceText);
    if (ranges.length === 0) {
        return;
    }

    // apply in reverse document order: extractContents()/insertNode() mutate the DOM, which would
    // invalidate the (node, offset) positions of any range that comes after it in the text
    for (let i = ranges.length - 1; i >= 0; i--) {
        wrapAbsoluteRange(codeElement, ranges[i]);
    }
}

function extractHighlightBlock(infoAttr: string): TriviaBlock | null {
    // the attribute already contains only the single h=... block (extracted server-side),
    // but parse defensively in case that ever changes
    for (const part of infoAttr.split(' ').filter(p => p.length > 0)) {
        if (!part.startsWith('h=')) {
            continue;
        }

        const result = parseTrivia(part);
        return result.success ? result.block : null;
    }

    return null;
}

function computeLineOffsets(sourceText: string): number[] {
    const offsets: number[] = [0];
    let index = sourceText.indexOf('\n');

    while (index !== -1) {
        offsets.push(index + 1);
        index = sourceText.indexOf('\n', index + 1);
    }

    return offsets;
}

function resolveAbsoluteRanges(block: TriviaBlock, lineOffsets: number[], sourceText: string): AbsoluteRange[] {
    const results: AbsoluteRange[] = [];

    for (const token of block.tokens) {
        resolveTokenRanges(token, lineOffsets, sourceText, results);
    }

    results.sort((a, b) => a.start - b.start);
    return mergeOverlapping(results);
}

function resolveTokenRanges(
    token: HighlightToken,
    lineOffsets: number[],
    sourceText: string,
    out: AbsoluteRange[]
): void {
    const lineCount = lineOffsets.length;

    for (let spanIndex = 0; spanIndex < token.lines.length; spanIndex++) {
        const lineSpec = token.lines[spanIndex];
        const isLastSpanInToken = spanIndex === token.lines.length - 1;

        const lineStart = resolveBound(lineSpec.start, 'start', lineCount);
        const lineEndInclusive = lineSpec.end === null ? lineStart : resolveBound(lineSpec.end, 'start', lineCount);

        if (lineStart >= lineCount || lineStart < 0) {
            continue;
        }

        const clampedEnd = Math.min(lineEndInclusive, lineCount - 1);

        for (let lineIndex = lineStart; lineIndex <= clampedEnd; lineIndex++) {
            const isLastLineOfSpan = lineIndex === clampedEnd;
            const appliesColumns =
                token.columns !== null && (token.isGrouped || (isLastSpanInToken && isLastLineOfSpan));

            const lineStartOffset = lineOffsets[lineIndex];
            const lineEndOffset = lineIndex + 1 < lineOffsets.length ? lineOffsets[lineIndex + 1] - 1 : sourceText.length;
            const lineLength = lineEndOffset - lineStartOffset;

            if (!appliesColumns) {
                if (lineLength > 0) {
                    out.push({start: lineStartOffset, end: lineEndOffset});
                }
                continue;
            }

            for (const columnRange of token.columns as readonly SpecRange[]) {
                const {start, end} = resolveRange(columnRange, lineLength);
                if (start >= end) {
                    continue;
                }

                out.push({start: lineStartOffset + start, end: lineStartOffset + end});
            }
        }
    }
}

function mergeOverlapping(ranges: AbsoluteRange[]): AbsoluteRange[] {
    if (ranges.length === 0) {
        return ranges;
    }

    const merged: AbsoluteRange[] = [ranges[0]];

    for (let i = 1; i < ranges.length; i++) {
        const current = ranges[i];
        const last = merged[merged.length - 1];

        if (current.start <= last.end) {
            last.end = Math.max(last.end, current.end);
        } else {
            merged.push(current);
        }
    }

    return merged;
}

function locateTextPosition(root: HTMLElement, targetOffset: number): { node: Text; offset: number } | null {
    const walker = document.createTreeWalker(root, NodeFilter.SHOW_TEXT);
    let consumed = 0;
    let node: Node | null;

    while ((node = walker.nextNode()) !== null) {
        const textNode = node as Text;
        const length = textNode.data.length;

        if (targetOffset <= consumed + length) {
            return {node: textNode, offset: targetOffset - consumed};
        }

        consumed += length;
    }

    return null;
}

function wrapAbsoluteRange(root: HTMLElement, range: AbsoluteRange): void {
    const startPos = locateTextPosition(root, range.start);
    const endPos = locateTextPosition(root, range.end);

    if (startPos === null || endPos === null) {
        return;
    }

    const domRange = document.createRange();
    domRange.setStart(startPos.node, startPos.offset);
    domRange.setEnd(endPos.node, endPos.offset);

    const mark = document.createElement('mark');
    mark.className = 'hl-mark';

    try {
        mark.appendChild(domRange.extractContents());
        domRange.insertNode(mark);
    } catch {
        // malformed/unexpected DOM shape for this range - degrade gracefully, skip this one mark
    }
}

export function applyAllCodeBlockHighlights(): void {
    document.querySelectorAll<HTMLElement>('code[data-highlight]').forEach(codeElement => {
        applyCodeBlockHighlights(codeElement);
    });
}
