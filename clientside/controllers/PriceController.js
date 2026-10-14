
var app = angular.module('InfinityLab');

app.controller('PriceController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService', '$timeout',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, $timeout) {
        $rootScope.AccordionName = $location.path();

        // $scope.Cites = [{ 'ID': 1, 'Name': 'Chennai' }, { 'ID': 248, 'Name': 'Cuddalore' }, { 'ID': 3, 'Name': 'Panruti' }, { 'ID': 4, 'Name': 't.nager' }];
        $scope.Navigation = $rootScope.GetNavigation();

        $scope.SearchRef = restService.SearchReference;

        $scope.BrandName = restService.Get('ReferencelistService/BRAND', true).then(function (results) {
            $scope.BrandName = results;
        });

        $scope.Locations = [];
        restService.Get('ReferencelistService/WHLOCATION', true).then(function (results) {
            results.push({ 'name': 'All Warehouse Location', 'id': -1 });
            results.push({ 'name': 'None', 'id': 0 });
            //$.each(results, function (k, v) {
            //    $scope.Locations.push(v);
            //})
            $scope.Locations = results;
        });

        restService.Get('ReferencelistService/COMPANY', true).then(function (results) {
            $scope.Company = results;
        });

        $scope.Colors = restService.Get('ReferencelistService/COLOUR', true).then(function (results) {
            $scope.Colors = results;
        });

        $scope.Supplier = [];
        restService.Get('ReferencelistService/SUPPLIER', true).then(function (results) {
            $scope.Supplier = results;
        });

        $scope.Patterns = restService.Get('ReferencelistService/PATTERN', true).then(function (results) {
            $scope.Patterns = results;
        });

        $scope.Sizes = restService.Get('ReferencelistService/SIZE', true).then(function (results) {
            $scope.Sizes = results;
        });

        $scope.Material = restService.Get('ReferencelistService/MATERIAL', true).then(function (results) {
            $scope.Material = results;
        });

        $scope.Styles = restService.Get('ReferencelistService/STYLE', true).then(function (results) {
            $scope.Styles = results;
        });

        $scope.Sections = restService.Get('ReferencelistService/DIVISION', true).then(function (results) {
            $scope.Sections = results;
        });

        $scope.Products = restService.Get('ReferencelistService/PRODUCTS', true).then(function (results) {
            $scope.Products = results;
            results.push({ 'name': 'All Core Products', 'id': -1 });
            results.push({ 'name': 'All Readymade Products', 'id': -2 });
            results.push({ 'name': 'None', 'id': 0 });
        });

        $scope.TypeList = restService.Get('ReferencelistService/TYPE', true).then(function (results) {
            $scope.TypeList = results;
        });

        $scope.SourceModes = [{ 'id': 0, 'name': 'Cost Include Purchase Discount' },
            { 'id': 1, 'name': 'Cost Exclude Purchase Discount' },
        { 'id': 2, 'name': 'Cost Include Purchase Tax' }];

        $scope.MarginModes = [{ 'id': 1, 'name': 'Markup' }, { 'id': 2, 'name': 'Markdown' },
        { 'id': 3, 'name': 'Fixed Margin' }, { 'id': 4, 'name': 'Fixed Price' }];

        $scope.DestinationModes = [{ 'id': 0, 'name': 'Include Tax' }, { 'id': 1, 'name': 'Exclude Tax' }];

        $scope.DiscountModes = [{ 'id': 1, 'name': 'None' },
        { 'id': 2, 'name': 'Amount' },
        { 'id': 3, 'name': 'Percentage' }];

        $scope.Entity = {

            "name": "",
            "brandid": 0,
            "colourid": 0,
            "productid": 0,
            "patternid": 0,
            "materialid": 0,
            "pricefrom": 0,
            "priceto": 0,
            "supplierid": 0,
            "locationid": 0,
            "sectionid": 0,
            "sizeid": 0,
            "styleid": 0,
            "typeid": 0,
            "pricingmode": 0,
            "priceingvalue": 0,
            "discountmode": 0,
            "discountvalue": 0,
            "createdon": "0001-01-01T00:00:00",
            "modifiedon": "0001-01-01T00:00:00",
            "createdby": 0,
            "modifiedby": 0,
            "isactive": true,
            "disabled": false,
            "version": 1
        };


        $scope.Price = (JSON.parse(JSON.stringify($scope.Entity)));

        $scope.ErrorStyle = "success";
        $scope.ShowSearch = false;
        $scope.SearchResult = [];

        $scope.SwitchPanel = function () {
            $scope.ShowSearch = !$scope.ShowSearch;
            $scope.Price = (JSON.parse(JSON.stringify($scope.Entity)));
            $scope.SearchResult = [];
        }

        $scope.ErrorMessage = "";

        //$scope.Preload = function () {

        //    request = {
        //        "Item": { id: 0 },
        //        "securitytoken": "String content"
        //    }

        //    var srequest = JSON.stringify(request);

        //    $http({
        //        method: 'POST',
        //        url: "PricecontrolService/Search",
        //        data: srequest
        //    }).success(function (result) {

        //        $scope.SearchResult = result;
        //    }).error(function (result) {
        //    });

        //}


        $scope.Clear = function () {
            $scope.Price = (JSON.parse(JSON.stringify($scope.Entity)));
        }

        $scope.Save = function () {

            if (($scope.Price.productid == 0 || typeof ($scope.Price.productid) == 'undefined') &&
                ($scope.Price.sectionid == 0 || typeof ($scope.Price.sectionid) == 'undefined') &&
                ($scope.Price.supplierid == 0 || typeof ($scope.Price.supplierid) == 'undefined')) {
                $scope['setFocusproductid']();
                $rootScope.Toaster.Error("Please Select Product or Supplier");
                return;
            }

            //if ($scope.Price.brandid == null || $scope.Price.brandid == 0) {
            //    $scope['setFocusbrandid']();
            //    $rootScope.Toaster.Error("Please Select Brand");
            //    return;
            //}

            //if ($scope.Price.pricingmode == 0) {

            //    $rootScope.Toaster.Error("Please select the pricing mode");
            //    return

            //}
            //if ($scope.Price.pricingmode == 2) {
            //    $scope.Price.pricingvalue = $scope.Price.pricingvalue_margin;
            //    if ($scope.Price.pricingvalue_margin == null || $scope.Price.pricingvalue_margin == 0) {
            //        $scope['setFocuspricingvalue_margin']();
            //        $rootScope.Toaster.Error("Please fill out Margin %");
            //        return
            //    }
            //}
            //if ($scope.Price.pricingmode == 3) {
            //    $scope.Price.pricingvalue = $scope.Price.pricingvalue_amount;
            //    if ($scope.Price.pricingvalue_amount == null || $scope.Price.pricingvalue_amount == 0) {
            //        $scope['setFocuspricingvalue_amount']();
            //        $rootScope.Toaster.Error("Please fill out Margin Amount");
            //        return
            //    }
            //}
            //if ($scope.Price.pricingmode == 4) {
            //    $scope.Price.pricingvalue = $scope.Price.pricingvalue_fixed;
            //    if ($scope.Price.pricingvalue_fixed == null || $scope.Price.pricingvalue_fixed == 0) {
            //        $scope['setFocuspricingvalue_fixed']();
            //        $rootScope.Toaster.Error("Please fill out Fixed Value");
            //        return
            //    }
            //}



            if ($scope.Price.discountmode > 1 && $scope.Price.discountvalue <= 0) {
                $rootScope.Toaster.Error("Please enter discount value");
                return;
            }

            var request = {
                "Item": $scope.Price,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "PricecontrolService/Save",
                data: srequest
            }).success(function (result) {

                //$scope.SearchResult = [];

                $.each(result, function (k, v) {
                    v.Display = $scope.GetProductName(v);
                    v.PricingMechanism = $scope.GetPricingMechanism(v);
                    v.DiscountMechanism = $scope.GetDiscountMechanism(v);
                    v.PriceRange = $scope.GetPriceRange(v);
                    $scope.SearchResult.push(v);
                });

                if ($scope.SearchResult.length == 1) {
                    $scope.ConfigGrid();
                }

                $scope.igrid.UpdateData($scope.SearchResult, '_id');

            }).error(function (result) {
            });

        }

        $scope.ManageSearchItem = function (action, item) {
            if (action == "Delete") {


                $.confirm({
                    title: 'Delete',
                    content: 'Do you want to delete ' + item.name + "?",
                    confirmButton: 'Yes',
                    confirmButtonClass: 'btn-info',
                    icon: 'fa fa-question-circle',
                    animation: 'scale',
                    animationClose: 'top',
                    opacity: 0.5,
                    buttons: {
                        formSubmit: {
                            text: 'Submit',
                            btnClass: 'btn-blue',
                            action: function () {
                                request = {
                                    "Item": item,
                                    "SecurityToken": "String content"
                                }
                                var srequest = JSON.stringify(request);
                                $http({
                                    method: 'POST',
                                    url: "PricecontrolService/Delete",
                                    data: srequest
                                }).success(function (result) {

                                    if (result) {
                                        $rootScope.Toaster("Deleted Successfully");
                                        $scope.SearchResult.splice($scope.SearchResult.indexOf(item), 1);
                                        $scope.igrid.UpdateData($scope.SearchResult, '_id');
                                    }

                                }).error(function (result) {

                                });
                            }
                        },
                        cancel: function () {
                            //close
                        },
                    },

                    onContentReady: function () {
                        // bind to events
                        var jc = this;
                        jc.$$formSubmit.trigger('focus');
                        this.$content.find('form').on('submit', function (e) {
                            // if the user submits the form by pressing enter in the field.
                            e.preventDefault();
                            jc.$$formSubmit.trigger('click'); // reference the button and click it
                        });
                    }
                });
            }
            else if (action == "Edit") {
                $scope.ShowSearch = false;
                $scope.Price = item;
            }
            else if (action == "Save") {
                request = {
                    "Item": item,
                    "SecurityToken": "String content"
                }
                var srequest = JSON.stringify(request);
                $http({
                    method: 'POST',
                    url: "PricecontrolService/Activate",
                    data: srequest
                }).success(function (result) {

                    if (result) {
                        $rootScope.Toaster.Info("Activated Successfully");
                    }
                }).error(function (result) {

                });
            }
        }

        $scope.ProductChanged = function (option) {
            if (option == "section") {
                $scope.Price.productid = 0;
            }
            else if (option == "product") {
                $scope.Price.sectionid = 0;
            } else if (option == "brand") {
                if (typeof ($scope.Price.productid) != 'undefined' &&
                    $scope.Price.productid > 0)
                    return;
            }

            if ($scope.Price.productid > 0 || $scope.Price.sectionid > 0 || $scope.Price.brandid > 0)
                $scope.Search();
        }

        $scope.GetPricingMechanism = function (item) {
            switch (item.pricingmode) {
                case 1:
                    return 'Markup %';
                case 2:
                    return 'Markdown %';
                case 3:
                    return 'Fixed Margin';
                case 4:
                    return 'Fixed Price';
            }
            return 'Error';
        };

        $scope.GetPriceRange = function (item) {
            if (item.pricefrom > 0 && item.priceto > 0)
                return '' + item.pricefrom + ' - ' + item.priceto;
            if (item.pricefrom > 0)
                return 'Min ' + item.pricefrom;
            if (item.priceto > 0)
                return 'Max ' + item.pricefrom;
            return '';
        };

        $scope.GetDiscountMechanism = function (item) {
            switch (item.discountmode) {
                case 1:
                    return 'No Discount';
                case 2:
                    return 'Fixed Value';
                case 3:
                    return 'Percentage';
            }
            return 'No Discount';
        };

        $scope.GetProductName = function (item) {
            var array = [];
            //if (item.supplierid != 'undefined' && item.supplierid != null && item.supplierid != 0)
            //    array.push($scope.SearchRef(item.supplierid, $scope.Supplier, ''));
            if (item.brandid != 'undefined' && item.brandid != null && item.brandid != 0)
                array.push($scope.SearchRef(item.brandid, $scope.BrandName, ''));
            if (item.colourid != 'undefined' && item.colourid != null && item.colourid != 0)
                array.push($scope.SearchRef(item.colourid, $scope.Colors, ''));
            if (item.sizeid != 'undefined' && item.sizeid != null && item.sizeid != 0)
                array.push($scope.SearchRef(item.sizeid, $scope.Sizes, ''));
            if (item.styleid != 'undefined' && item.styleid != null && item.styleid != 0)
                array.push($scope.SearchRef(item.styleid, $scope.Styles, ''));
            if (item.patternid != 'undefined' && item.patternid != null && item.patternid != 0)
                array.push($scope.SearchRef(item.patternid, $scope.Patterns, ''));
            if (item.materialid != 'undefined' && item.materialid != null && item.materialid != 0)
                array.push($scope.SearchRef(item.materialid, $scope.Material, ''));
            if (item.typeid != 'undefined' && item.typeid != null && item.typeid != 0)
                array.push($scope.SearchRef(item.typeid, $scope.TypeList, ''));

            var display = array.join('/');

            if (display == '')
                display = item.productid > 0 ? 'All ' + $scope.SearchRef(item.productid, $scope.Products, '') : 'All ' + $scope.SearchRef(item.sectionid, $scope.Sections, '');
            return display;
        };

        $scope.ConfigGrid = function (result) {
            if ($scope.igrid != null) {
                $scope.igrid.grid.destroy();
                $('#gridContainer').empty();
            }

            var igrid = new InfyGrid('gridContainer', '425px', '100%', true, false, true, 1);
            igrid.AddReferenceColumn('locationid', 'Locations', 120, $scope.Locations).defaultValue = 'All';
            igrid.AddReferenceColumn('supplierid', 'Supplier', 120, $scope.Supplier).defaultValue = 'All';;
            igrid.AddSimpleColumn('Display', 'Detail', 150);
            igrid.AddSimpleColumn('PriceRange', 'Price Range', 100);
            igrid.AddSimpleColumn('PricingMechanism', 'Price', 100);
            igrid.AddColumn(
                {
                    id: "nprice", name: "Value", field: "pricingvalue",
                    width: 60, cssClass: "cell-title", editor: Slick.Editors.Text,
                    'sortable': true
                }
            );
            igrid.AddSimpleColumn('DiscountMechanism', 'Discount', 100);
            igrid.AddColumn(
                {
                    id: "nprice", name: "Value", field: "discountvalue",
                    width: 60, cssClass: "cell-title", editor: Slick.Editors.Text,
                    'sortable': true
                }
            );
            //igrid.AddCustomColumn('disabled', '<input type="checkbox" style="z-index: auto;margin-left:5px">', 40,
            //    function (item, col) {
            //        if (item.disabled)
            //            return '<input type="checkbox" style="z-index: auto;margin-left:10px" checked>';
            //        else
            //            return '<input type="checkbox" style="z-index: auto;margin-left:10px">';
            //    });

            var commands = "<input type='button' class='btn btn-primary btn-xs' value='Save'/> &nbsp;"
            commands += "<input type='button' class='btn btn-info btn-xs' value='Edit'/> &nbsp;"
            commands += "<input type='button' class='btn btn-danger btn-xs' value='Delete'/> &nbsp;"


            igrid.AddButtonColumn('action', 'ACTION', 175, commands, function (e, args) {
                $scope.$apply(function () {
                    if ($(e.target).val() == 'Edit')
                        $scope.ManageSearchItem('Edit', args.item);
                    else if ($(e.target).val() == 'Delete')
                        $scope.ManageSearchItem('Delete', args.item);
                    else if ($(e.target).val() == 'Save')
                        $scope.ManageSearchItem('Save', args.item);
                });
            });

            igrid.options.autoEdit = true;
            igrid.Initialize();
            igrid.grid.onHeaderClick.subscribe(function (e, args) {
                if (args.column.id == 'edit' && $(e.target).is(":checkbox")) {
                    if (igrid.grid.getEditorLock().isActive() && !igrid.grid.getEditorLock().commitCurrentEdit()) {
                        e.preventDefault();
                        e.stopImmediatePropagation();
                        return;
                    }
                    if ($(e.target).is(":checked")) {
                        for (var i = 0; i < igrid.grid.getData().getLength(); i++) {
                            var item = igrid.grid.getDataItem(i);
                            if (!item.disabled) {
                                item.disabled = true;
                                $scope.ManageSearchItem('Save', item);
                            }
                        }
                    } else {
                        for (var i = 0; i < igrid.grid.getData().getLength(); i++) {
                            var item = igrid.grid.getDataItem(i);
                            if (item.disabled) {
                                item.disabled = false;
                                $scope.ManageSearchItem('Save', item);
                            }
                        }
                    }
                    igrid.grid.invalidate();
                    igrid.grid.render();
                    e.stopPropagation();
                    e.stopImmediatePropagation();
                }
            });

            igrid.grid.onClick.subscribe(function (e, args) {
                if (args.grid.getColumns()[args.cell].id == 'edit') {
                    item = args.grid.getDataItem(args.row);
                    item.disabled = !item.disabled && $(e.target).val() == 'on';
                    $scope.ManageSearchItem('Save', item);
                    $timeout(function () {
                        args.grid.invalidateRow(args.row);
                        args.grid.render();
                    }, 10);

                }
            });

            igrid.grid.onCellChange.subscribe(function (e, args) {
                var item = args.grid.getDataItem(args.row);
            });

            igrid.UpdateData([]);
            $scope.igrid = igrid;
        }

        $scope.Search = function () {

            $scope.ErrorMessage = '';
            request = {
                "Item": $scope.Price,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "PricecontrolService/Search",
                data: srequest
            }).success(function (result) {

                $scope.ConfigGrid();

                $scope.igrid.UpdateData([], '_id');
                if (angular.isArray(result)) {
                    $.each(result, function (k, v) {
                        v.Display = $scope.GetProductName(v);
                        v.PricingMechanism = $scope.GetPricingMechanism(v);
                        v.DiscountMechanism = $scope.GetDiscountMechanism(v);
                        v.PriceRange = $scope.GetPriceRange(v);
                    });
                    $scope.igrid.UpdateData(result, '_id');
                }
                else
                    $rootScope.Toaster.Info("No Results Found.");
            }).error(function (result) {
            });

        }

        $scope.exporttoExcel = function (infygrid, filename, tableid) {
            var title = $rootScope.UserInfo.CompanyName;
            var subtitle = 'MARGIN CONTROLLER - EXPORT'
            $scope.exportHref = infygrid.ExportExcel(infygrid.grid, title, subtitle);
            if ($scope.exportHref != null) {
                $timeout(function () {
                    var link = document.createElement('a');
                    link.download = filename + ".xls";
                    link.href = $scope.exportHref;
                    document.body.appendChild(link);
                    link.click();
                    link.remove();
                }, 100);
            }
        }

        $timeout(function () {
            $scope.ConfigGrid();
        }, 200);

    }]);
