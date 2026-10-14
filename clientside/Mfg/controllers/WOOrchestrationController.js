
var app = angular.module('InfinityLab');

app.controller('WOOrchestrationController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService', '$timeout',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, $timeout) {
        $scope.Navigation = $rootScope.GetNavigation();
        $rootScope.AccordionName = $location.path();
        $scope.SearchRef = restService.SearchReference;
        $scope.SearchCondition = { 'name': '' };

        $scope.Entity = {
            name: "",
            "bomsupport": "Yes",
            "mode": "Internal",
            "stagesupport": "Yes",
            stages: [],
            isactive: true,
            version: 1
        };

        $scope.WOOrchestration = (JSON.parse(JSON.stringify($scope.Entity)));

        $scope.ErrorStyle = "success";
        $scope.ShowSearch = false;
        $scope.SearchResult = [];
        $scope.CurrentStage = null;
        $scope.SwitchPanel = function () {
            $scope.ShowSearch = !$scope.ShowSearch;
            $scope.WOOrchestration = (JSON.parse(JSON.stringify($scope.Entity)));
        }

        $scope.ColorList = restService.Get('ReferencelistService/COLOUR', true).then(function (results) {
            $scope.ColorList = results;
        });

        $scope.MfgStages = restService.Get('ReferencelistService/MFGSTAGES', true).then(function (results) {
            $scope.MfgStages = results;
        });

        $scope.ConsumptionTypes = [{ id: 1, name: 'Material' }
            , { id: 2, name: 'Charges' }
        ];

        $scope.ConsumptionModes = [{ id: 1, name: 'Colour Based' }
            , { id: 2, name: 'Size Based' }
            , { id: 3, name: 'Colour/Size Based' }
            , { id: 4, name: 'Manual' }
            , { id: 5, name: 'All Colour/Size' }
        ];

        $scope.SizeList = restService.Get('ReferencelistService/SIZE', true).then(function (results) {
            $scope.SizeList = results;
        });

        $scope.Products = restService.Get('ReferencelistService/PRODUCTS', true).then(function (results) {
            $scope.Products = results;
        });
        
        $scope.BOMAllList = restService.Get('ReferencelistService/BOMLIST', true).then(function (results) {
            results.push({ 'id': -1, 'name': 'Dynamic' });
            $scope.BOMAllList = results;
        });
        $scope.BOMCategories = restService.Get('ReferencelistService/BOMCATEGORY', true).then(function (results) {
            $scope.BOMCategories = results;
        });

        $scope.StageJobcharges = [];
        $scope.JobCharges = restService.Get('ReferencelistService/FULL-JOBCHARGE', true).then(function (results) {
            $scope.JobCharges = results;
        });

        $scope.categoryChange = function () {
            if (typeof ($scope.CurrentBOM.categoryid) != 'undefined'
                && $scope.CurrentBOM.categoryid != null) {
                var BOMList = [];
                $.each($scope.BOMAllList, function (k, v) {
                    if (v.categoryid == $scope.CurrentBOM.categoryid)
                        BOMList.push(v);
                });
                $scope.BOMList = BOMList;
            }
        }

        $scope.corestagechange = function () {
            if (typeof ($scope.stage.corestageid) == 'undefined' || $scope.stage.corestageid == null || $scope.stage.corestageid == 0) {
                $scope.stage.corestageid = 0;
                $scope.stage.name = '';
            }
            else {
                $scope.stage.name = $scope.CoreStages.find(function (item) {
                    return $scope.stage.corestageid == item.id;
                }).name;
            }
        }

        $scope.TailoringMode = [{ id: 3, name: 'No' }, { id: 1, name: 'Work Order' }, { id: 2, name: 'Job Order' }];

        $scope.SizeModes = [{ id: 1, name: 'Size Range' }, { id: 2, name: 'Size Set' },
        { id: 3, name: 'Single Size' }, { id: 4, name: 'No Size' }];

        $scope.CoreStages = [
            { id: -1, name: 'BOM Process' , 'remarks':'BOM will be converted as another BOM' },
            { id: 1, name: 'Cutting' , 'remarks':'BOM will convert to Prefinish Product' },
            { id: 2, name: 'Stitching', 'remarks':'Prefinish product process'},
            { id: 3, name: 'Process/Packing', 'remarks':'End of Process, push to warehouse' }
        ];

        $scope.IOModes = [
        { id: 1, name: '1st Component Input' },
        { id: 2, name: '1st Component Output' },
        { id: 3, name: '2nd Component Input' },
        { id: 4, name: '2nd Component Output' },
        { id: 5, name: 'Product Input' },
        { id: 6, name: 'Product Output' }];

        $scope.CalculationType = [{ id: 0, name: 'Fixed' }, { id: 1, name: 'Multiply' },
        { id: 2, name: 'Divide' }];

        $scope.CalculationSource = [{ id: 1, name: 'Work Order' },
        { id: 2, name: 'Previouse Order Income' },
        { id: 3, name: 'Previouse Order Outcome' }]


        $scope.MaterialType = [{ id: 1, name: 'Component' }, { id: 2, name: 'Product' }];

        $scope.WarehouseLocations = restService.Get('ReferencelistService/WHLOCATION', true).then(function (results) {
            $scope.WarehouseLocations = results;
        });

        $scope.Department = restService.Get('ReferencelistService/MFRDEPARTMENT', true).then(function (results) {
            $scope.Department = results;
        });
        $scope.BOMCategory = restService.Get('ReferencelistService/BOMCATEGORY', true).then(function (results) {
            $scope.BOMCategory = results;
        });
        $scope.TaxList = restService.Get('ReferencelistService/PURCHASETAX', true).then(function (results) {
            $scope.TaxList = results;
        });
        $scope.ErrorMessage = "";

        $scope.BOMCoreTypes = [
            { 'id': 0, 'name': 'None' },
            { 'id': 1, 'name': 'Fabric' },
            { 'id': 2, 'name': 'Button' },
            { 'id': 3, 'name': 'Others' }
        ];

        $scope.AddItem = function () {
            if ($scope.CurrentBOM.iomode > 4) {
                $scope.CurrentBOM.iocategory = $scope.SearchRef($scope.CurrentBOM.productid, $scope.Products);
                $scope.CurrentBOM.ioname = $scope.SearchRef($scope.CurrentBOM.colourid, $scope.ColorList);
            }
            else {
                $scope.CurrentBOM.iocategory = $scope.SearchRef($scope.CurrentBOM.bomcategoryid, $scope.BOMCategories);
                $scope.CurrentBOM.ioname = $scope.SearchRef($scope.CurrentBOM.bomid, $scope.BOMAllList);
            }
            $scope.CurrentStage.bomitems.push(JSON.parse(JSON.stringify($scope.CurrentBOM)));
        }

        $scope.OnStageAction = function (action, list, item) {
            if (action == 'New') {
                $.each(list, function (k, v) {
                    v.selected = false;
                });
                list.push({
                    'index': list.length, 'name': 'Stage - ' + (list.length + 1), 'bomitems': [], 'selected': true
                });
                $scope.CurrentStage = list[list.length - 1];
            }
            if (action == 'DeleteItem') {
                list.splice(list.indexOf(item), 1);
            }
            if (action == 'EditItem') {
                list.splice(list.indexOf(item), 1);
                $scope.CurrentBOM = item;
            }
            if (action == 'Delete') {
                list.splice(list.indexOf(item), 1);
                $.each(list, function (k, v) {
                    v.index = k;
                });
                if (item.selected)
                    $scope.CurrentStage = null;
            }
            if (action == "MoveUp") {
                if (list.indexOf(item) > 0) {
                    var index = list.indexOf(item) - 1;
                    list.splice(list.indexOf(item), 1);
                    list.splice(index, 0, item);
                    $.each(list, function (k, v) {
                        v.index = k;
                    });
                }

            }
            if (action == "MoveDown") {
                if (list.indexOf(item) + 1 < list.length) {
                    var index = list.indexOf(item) + 1;
                    list.splice(list.indexOf(item), 1);
                    list.splice(index, 0, item);
                    $.each(list, function (k, v) {
                        v.index = k;
                    });
                }
            }
        }

        $scope.OnStageSelected = function (stage) {
            $.each($scope.WOOrchestration.stages, function (k, v) {
                v.selected = false;
            });
            stage.selected = true;
            $scope.CurrentStage = stage;
        }

        $scope.New = function () {
            $scope.WOOrchestration = (JSON.parse(JSON.stringify($scope.Entity)));
            $scope.ErrorMessage = "";
        }

        $scope.Save = function () {

            if ($scope.WOOrchestration.name == null || $scope.WOOrchestration.name == 0) {
                $scope['setFocusfirst']();
                $rootScope.Toaster.Error("Please Enter BOM Master Name");
                return;
            }


            request = {
                "Item": $scope.WOOrchestration,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "MfgworkfloworchestrationService/Save",
                data: srequest
            }).success(function (result) {

                if (result != null && result.id > 0) {

                    $rootScope.Toaster.Info("Saved Successfully.!");
                    $scope.WOOrchestration = (JSON.parse(JSON.stringify($scope.Entity)));
                }
                else {

                    $rootScope.Toaster.Error(result.ErrorMessage);

                }

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
                                    url: "MfgworkfloworchestrationService/Delete",
                                    data: srequest
                                }).success(function (result) {

                                    if (result) {
                                        $scope.ErrorMessage = "Successfully deleted";
                                        $scope.SearchResult.splice($scope.SearchResult.indexOf(item), 1);
                                        $scope.igrid.UpdateData($scope.SearchResult);
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
                $scope.WOOrchestration = item;
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
                url: "MfgworkfloworchestrationService/Search",
                data: srequest
            }).success(function (result) {

                $scope.SearchResult = result;
                $scope.WOOrchestrationConfig();
            }).error(function (result) {
            });
        }

        $scope.AddConsumption = function () {
            if ($scope.CurrentStage.bomitems == undefined ||
                $scope.CurrentStage.bomitems == null)
                $scope.CurrentStage.bomitems = [];

            $scope.CurrentStage.bomitems.push({
                'type': $scope.CurrentBOM.consumtiontype,
                'mode': $scope.CurrentBOM.consumtionmode,
                'categoryid': $scope.CurrentBOM.categoryid,
                'bomid': $scope.CurrentBOM.bomid,
                'cost': $scope.CurrentBOM.cost,
                'duration': $scope.CurrentBOM.duration,
                'chargeid': $scope.CurrentBOM.chargeid,
                'colourid': $scope.CurrentBOM.colourid,
                'sizeid': $scope.CurrentBOM.sizeid,
            });
        }

        $scope.MfgStageChanged = function () {
            var stage = $scope.MfgStages.find(function (item) {
                return item.id == $scope.CurrentStage.stageid;
            });
            if (stage && stage != null) {
                $scope.CurrentStage.name = stage.name;
            }

            var jobcharges = [];
            $.each($scope.JobCharges, function (k, v) {
                if (v.additionalinfo.mfgconfig &&
                    v.additionalinfo.mfgconfig.stageid &&
                    v.additionalinfo.mfgconfig.stageid == $scope.CurrentStage.stageid) {
                    jobcharges.push(v);
                }
            });
            $scope.StageJobcharges = jobcharges;
        }

        $scope.WOOrchestrationConfig = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '450px', '100%', true, false, true, 0);
            igrid.AddSimpleColumn('name', 'Name', 150);
            igrid.AddSimpleColumn('mode', 'Mode', 150);
            igrid.AddSimpleColumn('bomsupport', 'Bill Of Material', 100);
            igrid.AddSimpleColumn('stagesupport', 'Stages', 100);
            igrid.AddReferenceColumn('sizemode', 'Size', 100, $scope.SizeModes);

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
            $scope.igrid = igrid;
            $scope.igrid.UpdateData($scope.SearchResult);

        }

    }]);