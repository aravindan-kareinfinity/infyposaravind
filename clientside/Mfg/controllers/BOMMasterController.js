
var app = angular.module('InfinityLab');

app.controller('BOMMasterController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService', '$timeout',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, $timeout) {
        $scope.Navigation = $rootScope.GetNavigation();
        $rootScope.AccordionName = $location.path();
        $scope.SearchRef = restService.SearchReference;
        $scope.IsDialog = typeof ($scope.ngDialogData) != 'undefined' && $scope.ngDialogData.length > 0;

        $scope.Entity = {
            name: "",
            printingname: "",
            categoryid: "",
            uomid: "",
            colorid: "",
            width: "",
            taxid: "",
            isactive: true,
            colourlist: []
        };

        $scope.StatusList = [{ id: 1, name: 'Unapproved' }
            , { id: 2, name: 'Approved' },
        { id: 3, name: 'Deactivated' }];


        $scope.BOMMaster = (JSON.parse(JSON.stringify($scope.Entity)));

        $scope.ErrorStyle = "success";
        $scope.ShowSearch = false;
        $scope.SearchResult = [];

        if ($scope.ShowSearch)
            $scope.Navigation.SetMode('SEARCH');
        else 
            $scope.Navigation.SetMode('EDIT');

        $scope.SwitchPanel = function () {
            $scope.ShowSearch = !$scope.ShowSearch;
            $scope.BOMMaster = (JSON.parse(JSON.stringify($scope.Entity)));
        }
        $scope.SupplierList = restService.Get('ReferencelistService/SUPPLIER', true).then(function (results) {
            $scope.SupplierList = results;
        });
        $scope.UOMList = restService.Get('ReferencelistService/UOM', true).then(function (results) {
            $scope.UOMList = results;
        });
        $scope.BOMCategory = restService.Get('ReferencelistService/BOMCATEGORY', true).then(function (results) {
            $scope.BOMCategory = results;
        });
        $scope.TaxList = restService.Get('ReferencelistService/PURCHASETAX', true).then(function (results) {
            $scope.TaxList = results;
        });
        $scope.ColorList = restService.Get('ReferencelistService/COLOUR', true).then(function (results) {
            $scope.ColorList = results;
        });
        $scope.Products = restService.Get('ReferencelistService/PRODUCTS', true).then(function (results) {
            $scope.Products = results;
        });
        $scope.ErrorMessage = "";
        $scope.ShowSupplier = false;
        $scope.ShowColour = false;
        $scope.BOMCoreTypes = [
            { 'id': 0, 'name': 'None' },
            { 'id': 1, 'name': 'Fabric' },
            { 'id': 2, 'name': 'Accessories' },
            { 'id': 3, 'name': 'Yarn' },
            { 'id': 31, 'name': 'Beam' },
            { 'id': 32, 'name': 'Trim' },
            { 'id': 6, 'name': 'Button' },
            { 'id': 7, 'name': 'Tag' },
            { 'id': 8, 'name': 'Carton' },
            { 'id': 9, 'name': 'Label' },
            { 'id': 10, 'name': 'Zip' },
            { 'id': 11, 'name': 'Tab' },
            { 'id': 12, 'name': 'Bobbin' },
            { 'id': 4, 'name': 'Others' }
        ];

        $scope.ColourModes = [
            { 'id': 0, 'name': 'None' },
            { 'id': 1, 'name': 'Single' },
            { 'id': 2, 'name': 'Multiple' }
        ];

        $scope.SearchCondition = {
            name: ""
        }

        $scope.New = function () {
            $scope.BOMMaster = (JSON.parse(JSON.stringify($scope.Entity)));
            $scope.ErrorMessage = "";
        }
        $scope.SaveRequest = false;
        $scope.Save = function () {

            if ($scope.BOMMaster.name == null || $scope.BOMMaster.name == 0) {
                $scope['setFocusfirst']();
                $rootScope.Toaster.Error("Please Enter BOM Master Name");
                return;
            }

            if ($scope.SaveRequest) return;
            $scope.SaveRequest = true;
            request = {
                "Item": $scope.BOMMaster,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "BOMMasterService/Save",
                data: srequest
            }).success(function (result) {
                $scope.SaveRequest = false;
                if (result != null && result.id > 0) {

                    $rootScope.Toaster.Info("BOMMaster Saved Successfully.!");

                    if ($scope.IsDialog) {
                        $scope.closeThisDialog($scope.result);
                    }
                    else {
                        $scope.BOMMaster = (JSON.parse(JSON.stringify($scope.Entity)));
                    }

                }
                else {
                    $rootScope.Toaster.Error(result.ErrorMessage);
                }

            }).error(function (result) {
                $scope.SaveRequest = false;
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
                                    url: "BOMMasterService/Delete",
                                    data: srequest
                                }).success(function (result) {

                                    if (result) {
                                        $scope.ErrorMessage = "Successfully deleted";
                                        $scope.SearchResult.splice($scope.SearchResult.indexOf(item), 1);
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
                $scope.BOMMaster = item;
                $scope.Navigation.SetMode('EDIT');
            }
        }

        $scope.Search = function () {

            request = {
                "Item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "BOMMasterService/Search",
                data: srequest
            }).success(function (result) {

                $scope.SearchResult = result;
                $scope.BomMasterConfig();
            }).error(function (result) {
            });

        }


        $scope.BomMasterConfig = function () {
            $scope.selectedcount = 0;
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '450px', '100%', true, true, true, 0);
            igrid.options.rowHeight = 40;
            igrid.options.showHeaderRow = true;

            igrid.AddSimpleColumn('name', 'Name', 150);
            igrid.AddSimpleColumn('printingname', 'Printing Name', 150);
            igrid.AddReferenceColumn('categoryid', 'Category', 100, $scope.BOMCategory);
            igrid.AddReferenceColumn('uomid', 'UOM', 100, $scope.UOMList);
            igrid.AddReferenceColumn('colorid', 'Color', 100, $scope.ColorList);
            igrid.AddReferenceColumn('taxid', 'Tax', 100, $scope.TaxList);
            igrid.AddSimpleColumn('width', 'Width', 100);

            column = igrid.AddCustomColumn('imgageid', 'Image', 100, function (item, col) {
                if (item.imageid) {
                    if (item.imageid != '')
                        return '<div style="height: 100%;display: flex;align-items: center;justify-content: center;"><img style="width:100%;height:100%" src="' + col.urlprefix + item.imageid + '"/></div>';
                    return '<div style="height: 100%;display: flex;align-items: center;justify-content: center;"><img src="clientside/dist/img/no-image.png"/></div>';
                }
                return '';
            });
            column.urlprefix = 'File/Show/' + $scope.UserInfo.OrganizationID + '/';


            var commands = "";
            commands += "<input type='button' class='btn btn-primary btn-xs' value='Edit'/> &nbsp;"
            commands += "<input type='button' class='btn btn-danger btn-xs' value='Delete'/>"

            igrid.AddButtonColumn('action', 'ACTION', 100, commands, function (e, args) {
                $scope.$apply(function () {
                    if ($(e.target).val() == 'Edit')
                        $scope.ManageSearchItem('Edit', args.item);
                    else if ($(e.target).val() == 'Delete')
                        $scope.ManageSearchItem('Delete', args.item);
                });
            });

            igrid.options.autoEdit = true;
            igrid.Initialize();
            igrid.onSelectedRangesChanged = function (range, rowmodel) {
                $timeout(function () {
                    $scope.selectedcount = range.length;
                }, 10);
            };
            $scope.igrid = igrid;
            $scope.igrid.UpdateData($scope.SearchResult);
        }
        $scope.selectedcount = 0;
        $scope.brandfocus = function () {
            if ($scope.BOMMaster.supplierid && $scope.BOMMaster.supplierid != null &&
                $scope.BOMMaster.supplierid > 0) {
                $scope.setFocussupplierbrand();
            }
            else {
                $scope.setFocuscolourmode();
            }
        }

        $scope.ManageSupplier = function (command, value) {
            if (command == 'ADD') {
                $scope.BOMMaster.supplierlist = $scope.BOMMaster.supplierlist || [];
                $scope.BOMMaster.supplierlist.push({
                    'id': $scope.m_supplierid,
                    'design': $scope.m_design,
                    'brand': $scope.m_brand,
                    'rate': $scope.m_rate,
                    'min': $scope.m_min,
                    'max': $scope.m_max,
                });
                $scope.m_supplierid = '';
                $scope.m_design = '';
                $scope.m_brand = '';
                $scope.m_rate = '';
                $scope.m_min = '';
                $scope.m_max = '';
                $scope.setFocusc_supplierid();
            }
            else if (command == 'DELETE') {
                $scope.BOMMaster.supplierlist.splice($scope.BOMMaster.supplierlist.indexOf(value), 1);
            }
        }

        $scope.exporttoExcel = function (infygrid, filename, tableid) {
            var title = $rootScope.UserInfo.CompanyName;
            var subtitle = 'BOM Master';
            infygrid.ExportXLSX(infygrid.grid, title, subtitle);
        }

        $scope.ManageColour = function (command, value) {
            if (command == 'ADD') {
                $scope.BOMMaster.colourlist = $scope.BOMMaster.colourlist || [];
                $scope.BOMMaster.colourlist.push({ 'name': $scope.colourcode, 'id': $scope.colourid, 'romin': $scope.romin, 'romax': $scope.romax, });
                $scope.colourid = '';
                $scope.romin = '';
                $scope.romax = '';
                $scope.colourcode = '';
                $scope.setFocusc_colourcode();
            }
            else if (command == 'DELETE') {
                $scope.BOMMaster.colourlist.splice($scope.BOMMaster.colourlist.indexOf(value), 1);
            }
        }

        $scope.cameraitemuploaded = function (data, args) {
            $scope.BOMMaster.imageid = data;
        }

        $scope.imageitemuploaded = function (image, args) {
            args.item.timageid = null;
            $rootScope.CropImage(image, 'IMAGE', function (data) {
                args.item.imageid = data;
            }, null, { 'width': 200, 'height': 100 });
        }


        $scope.camerauploaded = function (data) {
            $scope.BOMMaster.imageid = data;
        }

        $scope.ShowStatusDialog = function () {

            var dialog = ngDialog.open({
                template: 'ChangeStatusDialog',
                data: [$scope],
                width: '400px',
                controller: ['$scope', '$timeout', function ($scope, $timeout) {
                    $scope.parent = $scope.ngDialogData[0];
                    $scope.ChangeStock = function () {
                        $scope.closeThisDialog($scope.statusid);
                    };
                }],
                className: 'ngdialog-theme-plain'
            });
            dialog.closePromise.then(function (data) {
                {
                    if (typeof (data.value) != 'undefined' && data.value != null
                        && data.value != '$escape' && data.value != '$document') {
                        var request = { 'statusid': data.value, ids: [] };

                        $.each($scope.igrid.GetSelectedRows(), function (k, v) {
                            request.ids.push(v.id);
                        });

                        var webrequest = {
                            "item": request,
                            "securitytoken": "String content"
                        }
                        var srequest = JSON.stringify(webrequest);
                        $http({
                            method: 'POST',
                            url: "BOMMasterService/ChangeStatus",
                            data: srequest
                        }).success(function (result) {
                            $rootScope.Toaster.Error('Attributes successfully changed.');
                        }).error(function (result) {
                        });
                    }
                }
            });
        }

        $scope.imageuploaded = function (image) {
            $scope.BOMMaster.imageid = null;
            $rootScope.CropImage(image, 'IMAGE', function (data) {
                $scope.BOMMaster.imageid = data;
            }, null, { 'width': 200, 'height': 100 });
        }

        $timeout(function () {
            if (typeof ($location.search().id) != 'undefined' &&
                $location.search().id.length > 0) {
                request = {
                    "Item": { 'id': $location.search().id },
                    "SecurityToken": "String content"
                }

                var srequest = JSON.stringify(request);

                $http({
                    method: 'POST',
                    url: "BOMMasterService/Search",
                    data: srequest
                }).success(function (result) {
                    $scope.ShowSearch = true;
                    if (angular.isArray(result)) {
                        $scope.ShowSearch = false;
                        $scope.BOMMaster = result[0];
                        $scope.Navigation.SetMode('EDIT');  
                    }

                }).error(function (result) {
                });
            }
            $scope.setFocusfirst();
        }, 100);

    }]);