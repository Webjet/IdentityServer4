var Webjet = Webjet || {};

Webjet.AdminLaunchpad = {
    containsKeyword: function(str, keyword) {
        return str.toUpperCase().indexOf(keyword.toUpperCase()) >= 0;
    },

    onSearchFieldKeyUp: function (linkGroupList) {
        var self = this;

        return function () {
            var keyword = $(this).val();

            $.each(linkGroupList, function (index, group) {
                var hasMatchingItem = false;
                
                // When group name contains keyword, show all links below this group
                if (group.listElement != null && self.containsKeyword(group.groupName, keyword)) {
                    group.listElement.show().addClass("toggle");
                    group.listElement.find("ul").show();
                    $.each(group.links, function (index, link) {
                        link.listElement.show();
                    });
                    return;
                }

                // Loop through links, show all matching links, hide otherwise
                $.each(group.links, function (index, link) {
                    if (self.containsKeyword(link.title, keyword)) {
                        hasMatchingItem = true;
                        link.listElement.show();
                    } else {
                        link.listElement.hide();
                    }
                });

                if (group.listElement != null) {
                    // Show/hide parent when based on matching children availability
                    group.listElement.toggle(hasMatchingItem);
                    if (hasMatchingItem) {
                        group.listElement.addClass("toggle");
                        group.listElement.find("ul").show();
                    }
                }
            });
        }
    },

    createLinkObjectList: function (list) {
        // First item is for links without group
        var linkGroupList = [{ listElement: null, groupName: null, links: [] }];

        // Loop through each list item
        $(list).each(function () {
            if ($(this).hasClass("has-children")) {
                // When list item has children, loop through each child
                var group = { listElement: $(this), groupName: $(this).find(".title").text(), links: [] }

                $(this).find("ul > li").each(function () {
                    var linkItem = {
                        listElement: $(this),
                        title: $(this).find(".link-title").text()
                    };
                    group.links.push(linkItem);
                });

                linkGroupList.push(group);

            } else {
                // Links without group go to index 0
                var linkItem = {
                    listElement: $(this),
                    title: $(this).find(".link-title").text()
                };
                linkGroupList[0].links.push(linkItem);
            }
        });

        return linkGroupList;
    }
};

$(function () {
    // Component init
    $(".multilevel-accordion").wjMultilevelAccordion({
        multiple: true
    });
    $("input.standard").wjTextField();


    // Render mobile menu
    var contentWrapper = $("#content-wrapper");
    var toggleNavigation = function () {
        contentWrapper.toggleClass("navigation-active");
    };
    contentWrapper.on("click", ".nav-toggle", toggleNavigation);
    $(window).resize(function () {
        // Close the mobile menu when users go to desktop size
        if (contentWrapper.hasClass("navigation-active") && !Webjet.Shared.isMobileSize()) {
            toggleNavigation();
        }
    });


    // Loop through each tab
    $("#site-list .tab-pane").each(function () {
        // Save links data into list of object
        var linkGroupList = Webjet.AdminLaunchpad.createLinkObjectList($(this).find("ul.multilevel-accordion > li"));

        // Add keyup event handler to search field
        $(this).find("input.standard").bind("keyup change", Webjet.AdminLaunchpad.onSearchFieldKeyUp(linkGroupList));
    });
});