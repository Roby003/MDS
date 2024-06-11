$(function () {
    var placeholderElement = $('#modal-placeholder');
    //load the modal in place of the placeholder div when button is clicked
    $('button[data-toggle="ajax-modal"]').click(function (event) {
        var url = $(this).data('url');
        $.get(url).done(function (data) {
            placeholderElement.html(data);
            placeholderElement.find('.modal').modal('show');
        });
    });
    $(function () {


        placeholderElement.on('click', '[data-save="modal"]', function (event) {
            event.preventDefault();
            //post form data

            formClass = '#comment_form'
            var form = $(formClass);

            var actionUrl = form.attr('action');
            var dataToSend = form.serialize();
            console.log(dataToSend);
            $.post(actionUrl, dataToSend).done(function (data) {
                var newForm = $(data).find(formClass);
                // Replace the content of the existing form
                form.replaceWith(newForm);
                // find IsValid input field and check it's value
                // if it's valid then hide modal window
                var isValid = newForm.find('[name="IsValid"]').val() == 'True';

                if (isValid) {
                        placeholderElement.find('.modal').modal('hide');
                       /* location.reload();*/
                    
                   
                    
                }

            });

        });
    })
})

const handlesBloomsTap = () => {
    const blooms = document.getElementsByClassName('bloom-card');
    for (let i = 0; i < blooms.length; i++) {
        const id = blooms[i].id;
        blooms[i].addEventListener('click', () => {
            if (!window.location.href.includes('/Blooms/Show')) {
                window.location.href = `/Blooms/Show/${id}`;
            }
        });
    }
}

window.onload = () => {
    handlesBloomsTap();
} 


