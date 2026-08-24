// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

document.addEventListener('DOMContentLoaded', function () {

    // 1. Handle Product Card Buttons (Quick Add, Qty = 1)
    const cartButtons = document.querySelectorAll('.ajax-cart-btn');
    cartButtons.forEach(button => {
        button.addEventListener('click', function (e) {
            e.preventDefault();
            const productId = this.getAttribute('data-product-id');

            submitToCart(productId, 1);
        });
    });

    // 2. Handle Details Page Form (Custom Qty)
    const detailsForm = document.getElementById('details-cart-form');
    if (detailsForm) {
        detailsForm.addEventListener('submit', function (e) {
            e.preventDefault(); // Stop the page from reloading!

            const productId = this.querySelector('input[name="productId"]').value;
            const quantity = this.querySelector('input[name="quantity"]').value;

            submitToCart(productId, quantity);
        });
    }

    // 3. The actual Fetch request
    function submitToCart(productId, quantity) {
        fetch(`/Cart/AddToCart?productId=${productId}&quantity=${quantity}`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            }
        })
            .then(response => response.json())
            .then(data => {
                if (data.success) {
                    // Success! You can trigger your Cart Sidebar to open here
                    alert(data.message); // Replace this with a nice UI Toast or SweetAlert

                    // TODO: Refresh the sidebar cart HTML so the new item shows up
                } else {
                    // Out of stock warning
                    alert("Error: " + data.message);
                }
            })
            .catch(error => console.error('Error:', error));
    }
});