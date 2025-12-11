const PAGE_SELECTOR = '#flow-root .flow-page';

function cleanup() {
    const root = document.getElementById('flow-root');
    if (!root) return;
    const pages = root.querySelectorAll('.flow-page');
    pages.forEach((page, index) => {
        if (index > 0) page.remove();
    });
}

function splitArticleToFit(page, pageHeightPx) {
    const lastArticle = page.querySelector('.article-flow:last-of-type');
    if (!lastArticle) return false;

    const content = lastArticle.querySelector('.article-content');
    if (!content) return false;

    const originalText = content.innerText;
    const words = originalText.split(' ');

    if (words.length < 10) {
        return false;
    }

    let low = 1;
    let high = words.length;
    let bestFit = 0;

    const restore = () => { content.innerText = originalText; };

    while (low <= high) {
        const mid = Math.floor((low + high) / 2);
        content.innerText = words.slice(0, mid).join(' ');

        const fits =
            lastArticle.offsetTop + lastArticle.offsetHeight <= pageHeightPx;

        if (fits) {
            bestFit = mid;
            low = mid + 1;
        } else {
            high = mid - 1;
        }
    }

    if (bestFit === 0 || bestFit >= words.length) {
        restore();
        return false;
    }

    const remainder = words.slice(bestFit).join(' ');
    content.innerText = words.slice(0, bestFit).join(' ');

    const newPage = document.createElement('div');
    newPage.className = 'flow-page';
    newPage.style.cssText = page.style.cssText;

    const continuation = lastArticle.cloneNode(true);
    const continuationContent = continuation.querySelector('.article-content');
    if (continuationContent) {
        continuationContent.innerText = remainder;
    }

    page.insertAdjacentElement('afterend', newPage);
    newPage.appendChild(continuation);

    return true;
}

export function paginate() {
    const firstPage = document.querySelector(PAGE_SELECTOR);
    if (!firstPage) return;

    cleanup();

    const pageHeightPx = firstPage.clientHeight;
    let page = firstPage;

    while (page) {
        while (page.scrollHeight > pageHeightPx) {
            const splitted = splitArticleToFit(page, pageHeightPx);
            if (!splitted) {
                const lastArticle = page.querySelector('.article-flow:last-of-type');
                if (!lastArticle) break;

                const newPage = document.createElement('div');
                newPage.className = 'flow-page';
                newPage.style.cssText = page.style.cssText;

                newPage.appendChild(lastArticle);
                page.insertAdjacentElement('afterend', newPage);
            }
        }

        page = page.nextElementSibling;
    }
}

export { cleanup };

