function setupDeleteAction(deleteButtonSelector, deleteUrl, categoryIdAttribute) {
    $(document).on("click", deleteButtonSelector, function (e) {
        e.preventDefault();

        var $deleteButton = $(this);
        var id = $deleteButton.data(categoryIdAttribute);

        $.confirm({
            title: 'Are you sure?',
            content: 'You won\'t be able to revert this!',
            icon: 'fa fa-warning',
            type: 'red',
            buttons: {
                confirm: {
                    text: 'Yes, delete it',
                    btnClass: 'btn-danger',
                    action: function () {
                        $.ajax({
                            url: deleteUrl,
                            type: 'POST',
                            data: { id: id },
                            beforeSend: function () {
                                $deleteButton.prop('disabled', true).html('<i class="fa fa-spinner fa-spin"></i> Deleting...');
                            },
                            success: function (result) {
                                var $parentRow = $deleteButton.closest('tr');
                                $parentRow.fadeOut(500, function () {
                                    $(this).remove();
                                });

                                $.alert({
                                    title: 'Success!',
                                    content: 'Your data has been securely deleted from our system.',
                                    type: 'green',
                                    icon: 'fa fa-check-circle',
                                    buttons: {
                                        ok: {
                                            text: 'Close',
                                            btnClass: 'btn-success'
                                        }
                                    }
                                });
                            },
                            error: function (error) {
                                console.error(error);
                                $.alert({
                                    title: 'Error!',
                                    content: 'An error occurred while deleting the data. Please try again later.',
                                    type: 'red',
                                    icon: 'fa fa-exclamation-circle',
                                    buttons: {
                                        ok: {
                                            text: 'Close',
                                            btnClass: 'btn-danger'
                                        }
                                    }
                                });
                            },
                            complete: function () {
                                $deleteButton.prop('disabled', false).html('Delete');
                            }
                        });
                    }
                },
                cancel: {
                    text: 'Cancel',
                    btnClass: 'btn-default'
                }
            }
        });
    });
}
