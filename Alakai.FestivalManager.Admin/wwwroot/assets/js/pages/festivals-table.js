window.alakaiFestivalsTable = {
    initialize: function (tableId) {
        window.setTimeout(function () {
            if (!window.jQuery || !window.jQuery.fn || !window.jQuery.fn.DataTable) {
                console.error('DataTables is not loaded. Check jQuery/DataTables scripts in App.razor.');
                return;
            }

            const selector = '#' + tableId;

            if (window.jQuery.fn.DataTable.isDataTable(selector)) {
                window.jQuery(selector).DataTable().destroy();
            }

            window.jQuery(selector).DataTable({
                responsive: true,
                pageLength: 10,
                lengthMenu: [5, 10, 25, 50],
                order: [[0, 'asc']]
            });
        }, 100);
    },
    destroy: function (tableId) {
        if (!window.jQuery || !window.jQuery.fn || !window.jQuery.fn.DataTable) {
            return;
        }

        const selector = '#' + tableId;

        if (window.jQuery.fn.DataTable.isDataTable(selector)) {
            window.jQuery(selector).DataTable().destroy();
        }
    },
    filterStatus: function (tableId, status) {
        if (!window.jQuery || !window.jQuery.fn || !window.jQuery.fn.DataTable) {
            return;
        }

        const selector = '#' + tableId;

        if (!window.jQuery.fn.DataTable.isDataTable(selector)) {
            return;
        }

        const table = window.jQuery(selector).DataTable();
        const value = status === 'all' ? '' : '^' + status + '$';
        table.column(3).search(value, true, false).draw();
    }
};
