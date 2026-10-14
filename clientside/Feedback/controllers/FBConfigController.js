//ReferencelistService/CITY
var app = angular.module('InfinityLab');

app.controller('FBConfigController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService', '$timeout',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, $timeout) {
        $scope.Navigation = $rootScope.GetNavigation();

        $scope.ErrorStyle = "success";
        $scope.ShowSearch = false;
        $scope.SearchResult = [];

        $scope.SwitchPanel = function () {
            $scope.ShowSearch = !$scope.ShowSearch;
        }

        $(document).ready(function () {
            $('#headersummernote').summernote({
                height: 135,
                minHeight: null,
                maxHeight: null,
                focus: true
            });
        });

        $(document).ready(function () {
            $('#responsesummernote').summernote({
                height: 135,
                minHeight: null,
                maxHeight: null,
                focus: true
            });
        });

        $scope.SearchRef = restService.SearchReference;

        $scope.Entity = {
            code: "",
            name: "",
            contactperson: "",
            contactno: "",
            address: "",
            isactive: true,
            version: 1,
            branches: [],
            html: {}
        };
        $scope.Organization = (JSON.parse(JSON.stringify($scope.Entity)));

        $scope.ErrorStyle = "success";
        $scope.ShowSearch = false;
        $scope.SearchResult = [];


        $scope.SearchCondition = {
            name: "",
            contactperson: ""
        }


        $scope.IsEmpty = function (value) {
            return typeof (value) == 'undefined' || value == '';
        }
        $scope.Save = function () {

            $scope.Organization.html = {};
            $scope.Organization.html.header = $('#headersummernote').summernote('code');
            $scope.Organization.html.response = $('#responsesummernote').summernote('code');

            if ($scope.Organization.html.header != null && $scope.Organization.html.header.indexOf('script') >= 0) {
                $rootScope.Toaster.Info("Headers contain script, can't support");
                return;
            }

            if ($scope.Organization.html.response != null && $scope.Organization.html.response.indexOf('script') >= 0) {
                $rootScope.Toaster.Info("Response contain script, can't support");
                return;
            }

            var srequest = JSON.stringify({ 'Item': $scope.Organization });

            $http({
                method: 'POST',
                url: "FBConfigService/Save",
                data: srequest
            }).success(function (result) {

                if (result != null) {
                    $rootScope.Toaster.Info("Feedback form Configuration Saved Successfully.!");
                    $scope.Organization.version = result.version;
                    $scope.Organization.modifiedon = result.modifiedon;
                    $scope.Organization.modifiedby = result.modifiedby;
                }


            }).error(function (result) {
            });

        }

        $scope.AddBranch = function () {
            if ($scope.Organization.branches == null) $scope.Organization.branches = [];
            $scope.Organization.branches.push({ 'code': $scope.branchcode, 'name': $scope.branchname });
        }

        $scope.DeleteBranch = function (item) {
            $scope.Organization.branches.splice($scope.Organization.branches.indexOf(item), 1);
        }

        $scope.SearchCurrent = function () {
            $http({
                method: 'GET',
                url: "FBConfigService/Current"
            }).success(function (result) {
                if (result != 'undefined' && result != null && result.html != 'undefined' && result.html != null) {
                    $scope.Organization = result;
                    $('#headersummernote').summernote('reset');
                    $('#headersummernote').summernote('code', result.html.header);

                    $('#responsesummernote').summernote('reset');
                    $('#responsesummernote').summernote('code', result.html.response);
                }
            }).error(function (result) {
            });
        }

        $scope.New = function () {
            $scope.Organization = (JSON.parse(JSON.stringify($scope.Entity)));
            $('#headersummernote').summernote('reset');
            $('#headersummernote').summernote('code', '<p></p>');

            $('#responsesummernote').summernote('reset');
            $('#responsesummernote').summernote('code', '<p></p>');
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
                                    url: "FBConfigService/Delete",
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
                $scope.Navigation.SetMode('EDIT');
                $scope.Organization = item;
                $('#headersummernote').summernote('reset');
                $('#headersummernote').summernote('code', item.html.header);

                $('#responsesummernote').summernote('reset');
                $('#responsesummernote').summernote('code', item.html.response);
            }
        }


        $scope.SearchCurrent();
    }
])
