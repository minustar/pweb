'use strict';

(function () {
    // Let's attach
    let items = document.querySelectorAll('button[data-xsampa-source]');
    for (let selected_btn of items) {
        selected_btn.addEventListener('click', async function (e) {
            const event_src = e.target;
            const sourceId = event_src.dataset.xsampaSource;
            const targetId = event_src.dataset.xsampaTarget || sourceId;
            const source = document.getElementById(sourceId);
            const target = document.getElementById(targetId);

            const result = await fetch('/api/xsampa/ipa', {
                method: 'POST',
                mode: 'no-cors',
                headers: {
                    'Content-Type': 'text/plain'
                },
                body: source.value
            });

            if (result.ok) {
                const text = await result.text();
                target.value = text;
                target.focus();
            }

            e.preventDefault();
            return false;
        });
    }
})();

async function attachConverters() {
    const CONVERT_CSS_SELECTOR = '[data-action="convert"]';
    const CONVERT_API_URL = '/api/text/convert/';
    const CONVERT_METHOD = 'POST';
    const CONVERT_MODE = 'no-cors';
    const CONTENT_TYPE_TEXT_PLAIN = 'text/plain';

    async function onclick_handler(e) {
        let inputs = find_inputs(e);

        let response = await call_api(inputs.source.value);
        if (response) {
            inputs.target.value = response;
            inputs.target.classList.remove('is-invalid');
            inputs.target.classList.add('is-valid');
        } else {
            inputs.target.classList.remove('is-valid');
            inputs.target.classList.add('is-invalid');
        }

        e.preventDefault();
        return false;
    }

    function find_inputs(e) {
        const src = e.currentTarget;
        const data = src.dataset;

        const source_id = data.source || data.target;
        const target_id = data.target || source;

        if (!source_id || !target_id) {
            throw 'Source and target inputs couldn\'t be located!.';
        }

        const source = document.getElementById(source_id);
        const target = document.getElementById(target_id);

        return { source, target };
    }

    /**
     * Calls the conversion API and returns a concerted string.
     * @param {string} src the string to converter.
     * @param {string} defaultReturn the default string returned if the call fails.
     * @returns the converted string.
     */
    async function call_api(src, defaultReturn = null) {
        var response = fetch(CONVERT_API_URL, {
            method: CONVERT_METHOD,
            mode:   CONVERT_MODE,
            headers: {
                'Content-Type': CONTENT_TYPE_TEXT_PLAIN
            },
            body:   src ||""
        });

        var result = await response;
        if (result.ok) {
            return await result.text();
        }

        return defaultReturn;
    }

    document.querySelectorAll(CONVERT_CSS_SELECTOR).forEach(it => it.addEventListener('click', onclick_handler));
}

attachConverters();


(function () {
    const API_XSAMPA_CONVERT = '/api/xsampa/ipa/';

})();
