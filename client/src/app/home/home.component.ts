import { HttpClient } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.css']
})
export class HomeComponent implements OnInit {
  registerMode = false;

  constructor() { }

  ngOnInit(): void {
    
  }
  registerToggle(){
    this.registerMode = true;
  }

  cancelRegisterMode(evetn: boolean){
    this.registerMode = evetn;
  }

}
