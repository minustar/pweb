(function () {
    /** API URL for X-SAMPA conversion. */
    const API_XSAMPA = '/api/utils/xsampa/';
    const API_CONVERT = '/api/utils/convert/';

    /**
     * Builds a <c>Request</c> object.
     * @param api the API endpoint.
     * @param text the text to convert.
     * @returns a Request object.
     */
    function build_request(api, text) {
        // Simply building a Request object.
        return {
            url: api,
            mode: 'no-cors',
            cache: 'no-cache',
            headers: {
                'Content-Type': 'text/plain'
            },
            body: text,
        };
    };

    /**
     * Calls the API.
     * @param api the API endpoint.
     * @param text the text to convert.
     * @returns the converted text.
     */
    async function call_api(api, text) {
        // Let's first build the request object.
        let request = build_request(api, text);

        try {
            // Calling fetch, and getting the Promise
            // result at once.
            let response = await fetch(request);

            // If the response's result is 200,
            // we simply read the response object's
            // body as raw text and return that.
            if (response.ok) {
                return await response.text();
            }
        }
        catch (ex) {
            // Should anything go wrong, we log the error.
            console.error(ex);
        }

        // If we get here, this means that we didn't get
        // a successful response from the call to the API,
        // or something went wrong along the way.
        return null;
    };

    /**
     * Tests if a value represents an object.
     * @param {any} value the value to test.
     * @returns {boolean} <c>true</c> if <c>value</c> is an Event.
     */
    function is_event(value) {
        // Value must be defined and not null
        if (!value)
            return false;

        // If it is an Event, it must be an object.
        if (typeof value !== 'object')
            return false;

        // Wit the property originalEvent.
        if (!value.hasOwnProperty('orignalEvent'))
            return false;

        return true;
    }

    /**
     * Locales and extract the source and
     * target inputs from the DOM.
     * @param {any} event the event which handler calls this method.
     */
    function get_inputs(event) {
        // Let's first check that the object we
        // use is actually an event.
        if (!is_event(event))
            throw 'Invalid arguments were passed.';

        // Now, let's get the Event's actual
        // registered source.
        const t = event.currentTarget;

        // The button click should at least have a
        // data-source attribute.
        // Optionally, a data-target attribute can
        // also be specified, if not, then we use
        // the same value as data-source.
        const source_id = t.dataset.source;
        const target_id = t.dataset.target || source_id;

        // Now, we retrieve the elements from the DOM.
        const source = document.getElementById(source_id);
        const target = document.getElementById(target_id);

        // If either once is mot available (note that
        // source and target CAN be the same), we
        // throw errors.
        if (!source) throw 'Source input is not found.';
        if (!target) throw 'Target input is not found.';

        // Finally, we return an object bearing both data.
        return { source, target };
    }

    /**
     * Processes the button click.
     * @param event the Event data.
     */
    async function onclick_xs_handler(event) {
        // We first get the input references.
        // Note that in some cases, soiurce and
        // target might be the same, but for
        // safety and flexibility reasons we
        // prefer to keep those separate.
        let { source, target } = get_inputs(event);

        // Let's validate the input, and if not,
        // we exit here.
        if (!source || !source.value)
            return;

        // Now, we call the API endpoint with the
        // value of the source input.
        let str = await call_api(API_XSAMPA, source.value);

        // We test the result.
        // On success: the target input's value is
        // set to the result and the is-valid CSS class
        // from Bootstrap gets applied.
        if (atr) {
            target.value = str;
            target.classList.add("is-valid");
            target.classList.remove("is-invalid");
        }
        else {
            target.classList.remove("is-valid");
            target.classList.add("is-invalid");
        }
        target.focus();

        // As this is a click event handler,
        // and thanks to some weird mechanics,
        // we should stop the event from bubbling
        // any further.
        event.preventDefault();
        return false;
    };

    // Let's apply the click handler on
    // the buttons that have a data-source
    // attribute, and a data-action='xsampa'.
    document
        .querySelectorAll('[data-source,data-action=\'xsampa\']')
        .forEach(function (it) {
            it.addEventListener('click', onclick_xs_handler);
        });
})();