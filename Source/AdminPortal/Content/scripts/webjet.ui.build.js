/* Common Webjet global javascript functions */
var Webjet = Webjet || {};

Webjet.Shared = {
    isMobileSize: function () {
        var maxMobileSize = 768;
        return ($(window).width() < maxMobileSize);
    },

    isMobileDevice: function () {
        return (Webjet.Shared.isMobileSize()) && ((typeof window.orientation !== "undefined") || (navigator.userAgent.indexOf('Mobile') !== -1));
    },

    addScriptTag: function (id, src) {
        var s = document.createElement('script');
        s.type = 'text/javascript';
        s.async = true;
        s.id = id;
        s.src = src;
        var head = document.getElementsByTagName('head')[0];
        head.appendChild(s);
    },

    addButtonLoader: function (element) {
        var btn = $(element);
        if (btn.find(".loader").length < 1) {
            btn.append($("<span>").addClass("loader"));

            if (btn.data("alt-text")) {
                btn.append($("<span>").addClass("loader-text").text(btn.data("alt-text")));
            }
        }

        btn.addClass("btn-loading");
    },

    removeButtonLoader: function (element) {
        $(element).removeClass("btn-loading");
    },

    getQueryParam: function (name) {
        var query = window.location.search.match(new RegExp('[?&]' + name + '=([^&#]*)'));
        return query && query[1];
    }
};

(function ($) {

    // Extension of Bootstrap Tooltip Widget
    $.extend($.fn.tooltip.Constructor.DEFAULTS, {
        delay: { show: 200, hide: 200 },
        html: true
    });

    // Extension of Bootstrap Popover Widget
    $.extend($.fn.popover.Constructor.DEFAULTS, {
        html: true,
        content: function () {
            var content = $(this).next(".popover-body");
            if (content.length > 0) {
                return content.html();
            }
            return null;
        }
    });

    // Extension of jQuery UI Selectmenu Widget
    $.widget("ui.selectmenu", $.ui.selectmenu, {
        _move: function (direction, event) {
            var item, next,
                filter = ".ui-menu-item";

            if (this.isOpen) {
                item = this.menuItems.eq(this.focusIndex);
            } else {
                item = this.menuItems.eq(this.element[0].selectedIndex);
            }

            // Skip disabled item
            filter += ":not(.ui-state-disabled)";

            if (direction === "first" || direction === "last") {
                next = item[direction === "first" ? "prevAll" : "nextAll"](filter).eq(-1);
            } else {
                next = item[direction + "All"](filter).eq(0);
            }

            if (next.length) {
                this.menuInstance.focus(event, next);
            }
        },

        open: function (event) {
            if (this.options.disabled) {
                return;
            }

            // If this is the first time the menu is being opened, render the items
            if (!this.menuItems) {
                this._refreshMenu();
            } else {

                // Menu clears focus on close, reset focus to selected item
                this.menu.find(".ui-state-focus").removeClass("ui-state-focus");
                this.menuInstance.focus(null, this._getSelectedItem());

                // Give selected item a unique class for styling purpose
                this.menuInstance.active.siblings().removeClass("selected-item");
                this.menuInstance.active.addClass("selected-item");
            }

            this.isOpen = true;
            this._toggleAttr();
            this._resizeMenu();
            this._position();

            this._on(this.document, this._documentClick);

            this._trigger("open", event);
        },

        _select: function (item, event) {
            this._super(item, event);
            this.element.trigger("change");
        }
    });


    // Extension of jQuery UI Autocomplete Widget
    $.widget("webjet.wjAutocomplete", $.ui.autocomplete, {
        _create: function () {
            this._super();
            this.widget().menu("option", "items", "> :not(.ui-state-disabled)");
        },

        _resizeMenu: function () {
            // Force autocomplete resultbox width to have the same width as input element
            var ul = this.menu.element;
            this.menu.element.outerWidth(this.element.outerWidth());
        },

        _renderMenu: function (ul, items) {
            var menu = this;
            var limit = parseInt(menu.options.limit) || 0;
            var message = menu.options.limitMessage || "Continue typing to refine options";

            $.each(items, function (index, item) {
                if (limit == 0 || index < limit) {
                    menu._renderItem(ul, item);
                }
                else {
                    // When limit is reached, then show limitMessage
                    var $msg = $("<li>");
                    $msg.addClass("ac-more ui-state-disabled").text(message);
                    $msg.appendTo(ul);
                    return false;
                }
            });
        },

        _renderItem: function (ul, item) {
            var menu = this;
            var type = menu.options.type || "none";
            var $list = $("<li>").data("ui-autocomplete-item", item);

            var defaultErrorMsg = {
                'flight': "No cities or airports were found. Please check your spelling.",
                'hotel': "No destinations were found. Please check your spelling."
            };

            // When NODATA is found, throw error message
            if (item.country == "NODATA") {
                var errorMessage = this.options.errorMessage || defaultErrorMsg[type];
                $list.addClass("ac-error").text(errorMessage);
                this.element.addClass("input-validation-error");

            } else {
                this.element.removeClass("input-validation-error");

                // Construct <li> element based on autocomplete type
                if (type === "flight") {
                    this.renderFlightItem($list, item);
                } else if (type === "hotel") {
                    this.renderHotelItem($list, item);
                } else {
                    this.renderItem($list, item);
                }
            }

            return $list.appendTo(ul);
        },

        renderItem: function($list, item) {
            var text = this.renderHighlight(item.value);
            var $listTop = $("<span>").addClass("ac-list-top").html(text);
            $list.append($("<a>").append($listTop));
        },

        renderFlightItem: function ($list, item) {
            var codeText = this.renderHighlight(item.code);
            var airportText = this.renderHighlight(item.airport);
            var cityText = this.renderHighlight(item.city);
            var countryText = this.renderHighlight(item.country);

            var $listTop = $("<span>").addClass("ac-list-top").html(airportText + " (" + codeText + ")");
            var $listBottom = $("<span>").addClass("ac-list-bottom").html(cityText + ", " + countryText);

            $list.append($("<a>").append($listTop, $listBottom));

            if (item.child) {
                $list.addClass("ac-child");
            }

            return $list;
        },

        renderHotelItem: function ($list, item) {
            var typeText = this.renderHighlight(item.type);
            var suburbText = this.renderHighlight(item.suburb);
            var cityText = this.renderHighlight(item.city);
            var countryText = this.renderHighlight(item.country);

            var $listTop = $("<span>").addClass("ac-list-top").html(suburbText);
            var $listBottom = $("<span>").addClass("ac-list-bottom").html(cityText + ", " + countryText);
            var $listExtra = $("<span>").addClass("ac-list-extra").html(typeText);

            $list.append($("<a>").append($listExtra, $listTop, $listBottom));

            return $list;
        },

        renderHighlight: function (string) {
            return string.replace(
                new RegExp("(?![^&;]+;)(?!<[^<>]*)(" + this.term.trim() + ")(?![^<>]*>)(?![^&;]+;)", "gi"),
                '<span class="ac-highlight">$1</span>'
            );
        }
    });

    // Extension of jQuery UI Slider Widget
    $.extend($.ui.slider.prototype, {
        _mouseCapture: function (event) {
            var position, normValue, distance, closestHandle, index, allowed, offset, mouseOverHandle,
                that = this,
                o = this.options;

            if (o.disabled) {
                return false;
            }

            this.elementSize = {
                width: this.element.outerWidth(),
                height: this.element.outerHeight()
            };
            this.elementOffset = this.element.offset();

            position = { x: event.pageX, y: event.pageY };
            normValue = this._normValueFromMouse(position);
            distance = this._valueMax() - this._valueMin() + 1;
            this.handles.each(function (i) {
                var thisDistance = Math.abs(normValue - (that.values(i) + (i > 0 ? 1 : -1)));
                if ((distance > thisDistance) ||
                    (distance === thisDistance &&
                        (i === that._lastChangedValue || that.values(i) === o.min))) {
                    distance = thisDistance;
                    closestHandle = $(this);
                    index = i;
                }
            });

            allowed = this._start(event, index);
            if (allowed === false) {
                return false;
            }
            this._mouseSliding = true;

            this._handleIndex = index;

            closestHandle
                .addClass("ui-state-active")
                .focus();

            offset = closestHandle.offset();
            mouseOverHandle = !$(event.target).parents().addBack().is(".ui-slider-handle");
            this._clickOffset = mouseOverHandle ? { left: 0, top: 0 } : {
                left: event.pageX - offset.left - (closestHandle.width() / 2),
                top: event.pageY - offset.top -
                    (closestHandle.height() / 2) -
                    (parseInt(closestHandle.css("borderTopWidth"), 10) || 0) -
                    (parseInt(closestHandle.css("borderBottomWidth"), 10) || 0) +
                    (parseInt(closestHandle.css("marginTop"), 10) || 0)
            };

            if (!this.handles.hasClass("ui-state-hover")) {
                this._slide(event, index, normValue);
            }
            this._animateOff = true;
            return true;
        }
    });

}(jQuery));
(function ($) {

    $.fn.wjTextField = function () {
        this.on("keyup", function (e) {
            $(this).siblings(".wj-alt-cross").toggle($(this).val() != "");
        });

        this.siblings(".wj-alt-cross").on("click", function (e) {
            $(this).siblings("input.standard").val("");
            $(this).siblings("input.standard").removeClass("input-validation-error");
            $(this).toggle();
        });

        return this;
    }


    $.fn.wjTextArea = function () {
        autosize(this);

        return this;
    }

    $.fn.wjAccordion = function () {
        this.on("click", ".title", function (e) {
            var accordion = $(this).closest(".accordion");
            var accordionItem = $(this).closest(".accordion-item");

            // accordion list should only be open 1 item at the time
            if (accordion.hasClass("accordion-list")) {
                var otherCordionItems = accordion.find(".toggle").not(accordionItem);
                otherCordionItems.find(".content").slideToggle();
                otherCordionItems.removeClass("toggle");
            }

            accordionItem.find(".content").slideToggle();
            accordionItem.toggleClass("toggle");
            e.preventDefault();
        });

        return this;
    }

    $.fn.wjMultilevelAccordion = function (options) {
        var settings = $.extend({
            // These are the defaults.
            multiple: false
        }, options);

        this.on("click", ".title", function (e) {
            var accordion = $(this).closest(".multilevel-accordion");
            var itemWithChildren = $(this).closest(".has-children");

            if (!settings.multiple) {
                // accordion list should only be open 1 item at the time
                var otherOpenedAccordionItems = accordion.find(".has-children.toggle").not(itemWithChildren);
                otherOpenedAccordionItems.find("> ul").slideToggle();
                otherOpenedAccordionItems.removeClass("toggle");
            }

            itemWithChildren.find("> ul").slideToggle();
            itemWithChildren.toggleClass("toggle");
            e.preventDefault();
        });
    }

    $.fn.wjImageCarousel = function () {
        var imageCarousel = this;
        var navText = ["<span class='wj-icon wj-previous'>", "<span class='wj-icon wj-next'>"];
        var currentItemCss = "current";
        var links = imageCarousel.find(".links");
        var carouselModal = imageCarousel.find(".modal");
        var images = carouselModal.find(".images");
        var thumbnails = carouselModal.find(".thumbnails");

        thumbnails.owlCarousel({
            items: 5,
            lazyLoad: true,
            pagination: false,
            responsive: false,
            scrollPerPage: true,
            responsiveRefreshRate: 50,
            navigation: true,
            rewindNav: true,
            navigationText: navText
        });

        images.owlCarousel({
            singleItem: true,
            lazyLoad: true,
            pagination: false,
            slideSpeed: 500,
            beforeMove: resetItem,
            afterAction: syncThumbnailPosition,
            responsiveRefreshRate: 50,
            navigation: true,
            rewindNav: true,
            navigationText: navText
        });

        function syncThumbnailPosition() {
            var current = this.currentItem;
            var thumbnailCarousel = thumbnails.data("owlCarousel");

            // highlight current thumbnail item
            thumbnailCarousel.$owlItems.removeClass(currentItemCss);
            thumbnailCarousel.$owlItems.eq(current).addClass(currentItemCss);

            var visibleItems = thumbnailCarousel.owl.visibleItems;
            if (visibleItems.indexOf(current) < 0) {
                // move next page
                if (current > visibleItems[visibleItems.length - 1]) {
                    thumbnails.trigger("owl.goTo", current - visibleItems.length + 5);
                }
                    // move previous page
                else if (current < visibleItems[0]) {
                    thumbnails.trigger("owl.goTo", current - 4);
                }
            }
        }

        function resetItem(el) {
            var currentItem = el.data("owlCarousel").$owlItems.eq(this.currentItem);

            // reset iframe content e.g. video
            var iframe = currentItem.find("iframe");
            iframe.attr("src", iframe.attr("src"));
        }

        thumbnails.off("click").on("click", ".owl-item", function (e) {
            var number = $(this).data("owlItem");
            images.trigger("owl.goTo", number);
        });

        // keyboard support for image carousel
        imageCarousel.off("keydown").on("keydown", function (e) {
            var isLeftKey = e.keyCode === 37;
            var isRightKey = e.keyCode === 39;

            if (isLeftKey || isRightKey) {
                var carousel = $(this).find(".owl-carousel.images").data("owlCarousel");
                if (carousel) {
                    isLeftKey ? carousel.prev() : carousel.next();
                }
            }
        });

        links.on("click", "img, .more", function () {
            var index = $(this).data("item") ? $(this).data("item") - 1 : 0;
            images.trigger("owl.goTo", index);

            carouselModal.modal("show");
        });

        return this;
    }

    $.fn.wjTooltip = function() {
        var tooltipClass = this.data("tooltip-class");
        
        this.tooltip({
            template: '<div class="tooltip ' + tooltipClass + '"><div class="tooltip-arrow"></div><div class="tooltip-inner"></div></div>',
            delay: { show: 200, hide: 200 },
            html: true
        });

        return this;
    }

    $.fn.wjPopover = function() {
        this.popover();
        return this;
    }
}(jQuery));