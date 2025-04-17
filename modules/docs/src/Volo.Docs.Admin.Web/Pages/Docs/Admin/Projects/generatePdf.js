var abp = abp || {};
$(function () {
    abp.modals.projectGeneratePdf = function () {
        
        var projectAppService = volo.docs.projects.docsProject;
        var pdfGeneratorAppService = volo.docs.documents.docsDocumentPdfGenerator;
        var projectAdminAppService = volo.docs.admin.projectsAdmin;
        
        var initModal = function (publicApi, args) {
            
        };

        $("#Versions").change(function () {
            var shortname = $("#ShortName").val();
            var version = $("#Version").val();
            projectAppService.getLanguageList(shortname, version).done(function (result) {
                $("#Language").empty();
                $.each(result.languages, function (index, item) {
                    $("#Language").append($(`<option value="${item.code}">${item.displayName}</option>`));
                });
            });
        })
        
        $("#GeneratePdfBtn").click(function () {
            var $btn = $(this);
            $btn.buttonBusy(true);
            var input = {
                projectId: $("#ProjectId").val(),
                version: $("#Version").val(),
                languageCode: $("#Language").val(),
            }
            tryDeletePdfFile(input);
            pdfGeneratorAppService.generatePdf(input, {
                abpHandleError : false,
                error: function (jqXHR) {
                    if (jqXHR.status === 200) {
                        abp.message.success("PDF generated successfully.");
                        $btn.buttonBusy(false);
                    } else {
                        abp.ajax.handleErrorStatusCode(jqXHR.status);
                    }
                }
            });
        })
        
        $("#GenerateAndDownloadPdfBtn").click(function () {
            var input = {
                projectId: $("#ProjectId").val(),
                version: $("#Version").val(),
                languageCode: $("#Language").val(),
            }
            tryDeletePdfFile(input);
            window.open(abp.appPath + 'api/docs/documents/pdf' + abp.utils.buildQueryString([{ name: 'projectId', value: input.projectId }, { name: 'version', value: input.version }, { name: 'languageCode', value: input.languageCode }]), '_blank');
        })
        
        function tryDeletePdfFile(input) {
            var forceToGenerate = $("#ForceToGenerate").is(":checked");
            if(forceToGenerate) {
                projectAdminAppService.deletePdfFile(input);
            }
        }
        
        return {
            initModal: initModal,
        };
    };
});
