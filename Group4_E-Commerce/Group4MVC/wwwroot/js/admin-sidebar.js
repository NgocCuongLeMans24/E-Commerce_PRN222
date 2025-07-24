//// Admin Sidebar JavaScript
//document.addEventListener("DOMContentLoaded", () => {
//    const sidebar = document.querySelector(".admin-sidebar")
//    const sidebarOverlay = document.getElementById("sidebarOverlay")
//    const mobileSidebarToggle = document.getElementById("mobileSidebarToggle")
//    const sidebarToggle = document.getElementById("sidebarToggle")
//    const bootstrap = window.bootstrap // Declare bootstrap variable
//    const loadCss = window.loadCss // Declare loadCss variable

//    //// Mobile sidebar toggle
//    //if (mobileSidebarToggle) {
//    //    mobileSidebarToggle.addEventListener("click", () => {
//    //        sidebar.classList.add("active")
//    //        sidebarOverlay.classList.add("active")
//    //        document.body.style.overflow = "hidden"
//    //    })
//    //}

//    //// Close sidebar on mobile
//    //if (sidebarToggle) {
//    //    sidebarToggle.addEventListener("click", () => {
//    //        sidebar.classList.remove("active")
//    //        sidebarOverlay.classList.remove("active")
//    //        document.body.style.overflow = ""
//    //    })
//    //}

//    // Close sidebar when clicking overlay
//    if (sidebarOverlay) {
//        sidebarOverlay.addEventListener("click", () => {
//            sidebar.classList.remove("active")
//            sidebarOverlay.classList.remove("active")
//            document.body.style.overflow = ""
//        })
//    }

//    // Handle window resize
//    window.addEventListener("resize", () => {
//        if (window.innerWidth >= 992) {
//            sidebar.classList.remove("active")
//            sidebarOverlay.classList.remove("active")
//            document.body.style.overflow = ""
//        }
//    })

//    // Auto-dismiss alerts
//    const alerts = document.querySelectorAll(".alert")
//    alerts.forEach((alert) => {
//        setTimeout(() => {
//            if (alert && alert.parentNode) {
//                alert.style.opacity = "0"
//                alert.style.transform = "translateY(-10px)"
//                setTimeout(() => {
//                    if (alert.parentNode) {
//                        alert.parentNode.removeChild(alert)
//                    }
//                }, 300)
//            }
//        }, 5000)
//    })

//    // Smooth scrolling for sidebar navigation
//    const navLinks = document.querySelectorAll(".nav-link:not(.disabled)")
//    navLinks.forEach((link) => {
//        link.addEventListener("click", (e) => {
//            // Close mobile sidebar when navigating
//            if (window.innerWidth < 992) {
//                sidebar.classList.remove("active")
//                sidebarOverlay.classList.remove("active")
//                document.body.style.overflow = ""
//            }
//        })
//    })

//    // Add loading state to buttons
//    const actionButtons = document.querySelectorAll(".btn-action, .btn-secondary")
//    actionButtons.forEach((button) => {
//        button.addEventListener("click", () => {
//            if (!button.disabled) {
//                const originalText = button.innerHTML
//                button.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Loading...'
//                button.disabled = true

//                // Re-enable after 3 seconds (fallback)
//                setTimeout(() => {
//                    button.innerHTML = originalText
//                    button.disabled = false
//                }, 3000)
//            }
//        })
//    })

//    // Enhanced modal handling
//    window.loadCss = (href) => {
//        if (!document.querySelector(`link[href="${href}"]`)) {
//            const link = document.createElement("link")
//            link.rel = "stylesheet"
//            link.href = href
//            document.head.appendChild(link)
//        }
//    }

//    //// Global modal functions
//    //window.openCustomerModal = (customerId = null) => {
//    //    showLoadingSpinner()
//    //    fetch(`/Admin/GetCustomerModal?id=${customerId || ""}`)
//    //        .then((response) => {
//    //            if (!response.ok) throw new Error("Network response was not ok")
//    //            return response.text()
//    //        })
//    //        .then((html) => {
//    //            hideLoadingSpinner()
//    //            document.getElementById("modalContainer").innerHTML = html
//    //            const modal = new bootstrap.Modal(document.getElementById("customerModal"))
//    //            loadCss("/css/customer-management/create.css")
//    //            modal.show()
//    //        })
//    //        .catch((error) => {
//    //            hideLoadingSpinner()
//    //            console.error("Error:", error)
//    //            showAlert("Failed to load customer form.", "error")
//    //        })
//    //}

//    //window.openEmployeeModal = (employeeId = null) => {
//    //    showLoadingSpinner()
//    //    fetch(`/Admin/GetEmployeeModal?id=${employeeId || ""}`)
//    //        .then((response) => {
//    //            if (!response.ok) throw new Error("Network response was not ok")
//    //            return response.text()
//    //        })
//    //        .then((html) => {
//    //            hideLoadingSpinner()
//    //            document.getElementById("modalContainer").innerHTML = html
//    //            const modal = new bootstrap.Modal(document.getElementById("employeeModal"))
//    //            loadCss("/css/employee-management/create.css")
//    //            modal.show()
//    //        })
//    //        .catch((error) => {
//    //            hideLoadingSpinner()
//    //            console.error("Error:", error)
//    //            showAlert("Failed to load employee form.", "error")
//    //        })
//    //}

//    // Utility functions
//    function showLoadingSpinner() {
//        const spinner = document.createElement("div")
//        spinner.id = "globalSpinner"
//        spinner.className = "global-spinner"
//        spinner.innerHTML = `
//            <div class="spinner-backdrop">
//                <div class="spinner-content">
//                    <i class="fas fa-spinner fa-spin fa-2x"></i>
//                    <p>Loading...</p>
//                </div>
//            </div>
//        `
//        document.body.appendChild(spinner)
//    }

//    function hideLoadingSpinner() {
//        const spinner = document.getElementById("globalSpinner")
//        if (spinner) {
//            spinner.remove()
//        }
//    }

//    function showAlert(message, type = "info") {
//        const alertClass = type === "error" ? "alert-danger" : "alert-success"
//        const iconClass = type === "error" ? "fa-exclamation-circle" : "fa-check-circle"

//        const alert = document.createElement("div")
//        alert.className = `alert ${alertClass} alert-dismissible fade show`
//        alert.innerHTML = `
//            <i class="fas ${iconClass} me-2"></i>
//            ${message}
//            <button type="button" class="close" data-dismiss="alert">
//                <span>&times;</span>
//            </button>
//        `

//        const content = document.querySelector(".admin-content")
//        if (content) {
//            content.insertBefore(alert, content.firstChild)
//        }
//    }

//    // Auto-update badge counts (if needed)
//    function updateBadgeCounts() {
//        // This can be implemented to fetch real-time counts via AJAX
//        // For now, it's just a placeholder
//    }

//    // Initialize tooltips if Bootstrap is available
//    if (typeof bootstrap !== "undefined") {
//        var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'))
//        var tooltipList = tooltipTriggerList.map((tooltipTriggerEl) => new bootstrap.Tooltip(tooltipTriggerEl))
//    }
//})

//// Global spinner styles
//const spinnerStyles = `
//.global-spinner {
//    position: fixed;
//    top: 0;
//    left: 0;
//    width: 100%;
//    height: 100%;
//    z-index: 9999;
//}

//.spinner-backdrop {
//    background: rgba(0, 0, 0, 0.5);
//    width: 100%;
//    height: 100%;
//    display: flex;
//    align-items: center;
//    justify-content: center;
//}

//.spinner-content {
//    background: white;
//    padding: 2rem;
//    border-radius: 0.5rem;
//    text-align: center;
//    box-shadow: 0 10px 25px rgba(0, 0, 0, 0.2);
//}

//.spinner-content i {
//    color: #3b82f6;
//    margin-bottom: 1rem;
//}

//.spinner-content p {
//    margin: 0;
//    color: #64748b;
//    font-weight: 500;
//}
//`

//// Inject spinner styles
//const styleSheet = document.createElement("style")
//styleSheet.textContent = spinnerStyles
//document.head.appendChild(styleSheet)
