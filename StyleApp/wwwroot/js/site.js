// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

const updateSelectionButton = (formId, buttonId, emptyLabel, selectedLabel) => {
    const form = document.getElementById(formId);
    const button = document.getElementById(buttonId);

    if (!form || !button) {
        return;
    }

    const checkboxes = Array.from(document.querySelectorAll('input[name="itemIds"]'))
        .filter(checkbox => checkbox.form === form);

    const update = () => {
        const selectedCount = checkboxes.filter(checkbox => checkbox.checked).length;
        button.disabled = selectedCount === 0;
        // If there's a nested element like a span in the original button, replace whole textContent to keep label simple.
        button.textContent = selectedCount === 0 ? emptyLabel : `${selectedLabel} (${selectedCount}) →`;
    };

    checkboxes.forEach(checkbox => checkbox.addEventListener("change", update));
    update();
};

updateSelectionButton("suggestionSelectionForm", "wardrobeIdeasButton", "Select pieces first", "Find outfit ideas");
updateSelectionButton("suggestionsForm", "suggestionsIdeasButton", "Select pieces first", "Show me ideas");
