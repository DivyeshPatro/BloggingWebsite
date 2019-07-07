import { AuthenticationserviceService } from './../shared/authenticationservice.service';
import { FormGroup, FormBuilder, Validators } from '@angular/forms';
import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { trigger, transition, style, animate } from '@angular/animations';

@Component({
  selector: 'app-blog-login',
  templateUrl: './blog-login.component.html',
  styleUrls: ['./blog-login.component.css'],
  animations: [
    trigger('slide',[
      transition("void => *",[
        style({transform:'translateX(35px)',opacity:0}),
        animate('0.6s')
      ])
    ]),
    trigger('scale',[
      transition("void => *",[
        style({transform:'scale(0.5)'}),
        animate('0.6s')
      ])
    ])
  ]
})
export class BlogLoginComponent implements OnInit {

  loginForm : FormGroup;
  constructor(private fb:FormBuilder,private authService : AuthenticationserviceService,private router:Router) { }

  ngOnInit() {
    this.loginForm = this.fb.group({
      username : ['',[Validators.required,Validators.pattern("^[\\w\-\\.]{2,20}$")]],
      password : ['',Validators.required]
    });
  }

  onSubmit(){
    if(this.loginForm.invalid){
      alert("You've entered invalid data.");
      return;
    }
    else{
      let user = {
        username : this.loginForm.controls.username.value,
        password : this.loginForm.controls.password.value
      };
      this.authService.logIn(user).subscribe(result => {
          localStorage.setItem('loggedUser',JSON.stringify(result.body));
          this.router.navigate(['/home']);
      }, error => {
        alert(error.statusText);
      });
    }
  }

}