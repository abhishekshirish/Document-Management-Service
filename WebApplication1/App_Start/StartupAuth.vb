Imports System.IdentityModel.Claims
Imports System.Linq
Imports System.Threading.Tasks
Imports Microsoft.Owin.Extensions
Imports Microsoft.Owin.Security
Imports Microsoft.Owin.Security.Cookies
Imports Microsoft.Owin.Security.OpenIdConnect
Imports Owin

Partial Public Class Startup
    Private Shared clientId As String = ConfigurationManager.AppSettings("ida:ClientId")
    Private Shared aadInstance As String = EnsureTrailingSlash(ConfigurationManager.AppSettings("ida:AADInstance"))

    Private Shared authority As String = aadInstance & "common"

    Public Sub ConfigureAuth(app As IAppBuilder)
        app.SetDefaultSignInAsAuthenticationType(CookieAuthenticationDefaults.AuthenticationType)

        app.UseCookieAuthentication(New CookieAuthenticationOptions())

        ' instead of using the default validation (validating against a single issuer value, as we do in line of business apps), 
        ' we inject our own multitenant validation logic
        app.UseOpenIdConnectAuthentication(New OpenIdConnectAuthenticationOptions() With {
              .ClientId = clientId,
              .Authority = authority,
              .TokenValidationParameters = New Microsoft.IdentityModel.Tokens.TokenValidationParameters() With {
                  .ValidateIssuer = False
              },
              .Notifications = New OpenIdConnectAuthenticationNotifications() With
              {
                  .SecurityTokenValidated = Function(context)
                                                'If your authentication logic is based on users
                                                Return Task.FromResult(0)
                                            End Function,
                  .AuthenticationFailed = Function(context)
                                              ' Pass in the context back to the app
                                              context.HandleResponse()
                                              ' Suppress the exception
                                              Return Task.FromResult(0)
                                          End Function
              }
        })

        'This makes any middleware defined above this line run before the Authorization rule is applied in web.config
        app.UseStageMarker(PipelineStage.Authenticate)
    End Sub

    Private Shared Function EnsureTrailingSlash(ByRef value As String) As String
        If (IsNothing(value)) Then
            value = String.Empty
        End If

        If (Not value.EndsWith("/", StringComparison.Ordinal)) Then
            Return value & "/"
        End If

        Return value
    End Function
End Class